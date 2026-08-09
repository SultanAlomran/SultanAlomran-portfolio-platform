import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';

const root=process.cwd();
const input=path.join(root,'test-results','results.json');
const output=path.join(root,'test-results','telemetry.json');
if(!fs.existsSync(input)){console.warn('No Playwright JSON report exists; telemetry was not generated.');process.exit(0);}
const report=JSON.parse(fs.readFileSync(input,'utf8'));
const args=process.argv.slice(2);const value=flag=>{const i=args.indexOf(flag);return i<0?undefined:args[i+1];};
const now=new Date();const isGitHub=process.env.GITHUB_ACTIONS==='true';
const executionMode={standard:0,visual:1,record:2,'full-recording':2}[value('--mode')??process.env.E2E_MODE??'standard']??0;
const browser=value('--browser')??process.env.E2E_BROWSER??'chromium';const requestedFeature=value('--feature')??process.env.E2E_FEATURE??'all';
const runUrl=isGitHub?`${process.env.GITHUB_SERVER_URL}/${process.env.GITHUB_REPOSITORY}/actions/runs/${process.env.GITHUB_RUN_ID}`:null;
let prNumber=null;if(process.env.GITHUB_EVENT_PATH&&fs.existsSync(process.env.GITHUB_EVENT_PATH)){try{prNumber=JSON.parse(fs.readFileSync(process.env.GITHUB_EVENT_PATH,'utf8')).pull_request?.number??null;}catch{}}
const tests=[];const artifacts=[];
const statusNumber=status=>({passed:0,failed:1,skipped:2,timedOut:3,interrupted:4}[status]??4);
const visit=suite=>{for(const child of suite.suites??[])visit(child);for(const spec of suite.specs??[])for(const test of spec.tests??[]){
 const results=test.results??[];const final=results.at(-1)??{};const correlation=`${test.projectName}:${spec.file}:${spec.line}:${spec.title}`;
 const feature=featureFrom(spec.file,requestedFeature);const suiteName=[...(suite.titlePath??[]),suite.title].filter(Boolean).join(' › ');
 const started=final.startTime?new Date(final.startTime):null;const completed=started?new Date(started.getTime()+(final.duration??0)):null;
 tests.push({feature,suite:suiteName,testName:spec.title,projectArea:spec.file?.includes('/public/')?'Public':'Admin',browser:test.projectName??browser,viewport:null,status:statusNumber(final.status),durationMs:final.duration??0,retryCount:Math.max(0,results.length-1),isFlaky:test.status==='flaky',errorType:final.error?.name??null,errorSummary:(final.error?.message??final.error?.stack??null)?.slice(0,2000)??null,sourceFile:spec.file??null,startedAtUtc:started?.toISOString()??null,completedAtUtc:completed?.toISOString()??null,correlationKey:correlation});
 for(const attachment of final.attachments??[])if(attachment.path)artifacts.push(artifact(attachment.path,attachment.contentType,correlation,feature,test.projectName??browser));
 }};
for(const suite of report.suites??[])visit(suite);
const addGlobal=(relative,type,mime)=>{const full=path.join(root,relative);if(fs.existsSync(full))artifacts.push({testCaseCorrelationKey:null,artifactType:type,provider:isGitHub?0:2,providerArtifactId:isGitHub?path.basename(relative):null,name:path.basename(relative),mimeType:mime,externalUrl:runUrl,storagePath:relative.replaceAll('\\','/'),sizeBytes:fs.statSync(full).isFile()?fs.statSync(full).size:null,createdAtUtc:now.toISOString(),expiresAtUtc:isGitHub?new Date(now.getTime()+14*86400000).toISOString():null,availabilityStatus:0,browser,feature:requestedFeature});};
addGlobal('test-results/results.json',5,'application/json');addGlobal('test-results/junit.xml',4,'application/xml');
if(fs.existsSync(path.join(root,'playwright-report')))artifacts.push({testCaseCorrelationKey:null,artifactType:0,provider:isGitHub?0:2,providerArtifactId:isGitHub?'playwright-report':null,name:'Playwright HTML report',mimeType:'text/html',externalUrl:runUrl,storagePath:'playwright-report/',sizeBytes:null,createdAtUtc:now.toISOString(),expiresAtUtc:isGitHub?new Date(now.getTime()+14*86400000).toISOString():null,availabilityStatus:0,browser,feature:requestedFeature});
const completed=new Date();const failed=tests.some(x=>x.status===1||x.status===3||x.status===4);
const request={provider:isGitHub?1:0,providerRunId:isGitHub?process.env.GITHUB_RUN_ID:`local-${completed.toISOString().replaceAll(/[-:.TZ]/g,'')}`,workflowName:process.env.GITHUB_WORKFLOW??'Local Playwright',workflowRunNumber:Number(process.env.GITHUB_RUN_NUMBER)||null,branch:process.env.GITHUB_HEAD_REF||process.env.GITHUB_REF_NAME||process.env.GIT_BRANCH||'local',commitSha:process.env.GITHUB_SHA||process.env.GIT_COMMIT||'local',pullRequestNumber:prNumber,trigger:process.env.GITHUB_EVENT_NAME??'local',executionMode,status:failed?2:1,startedAtUtc:new Date(completed.getTime()-tests.reduce((sum,x)=>sum+x.durationMs,0)).toISOString(),completedAtUtc:completed.toISOString(),repositoryUrl:isGitHub?`${process.env.GITHUB_SERVER_URL}/${process.env.GITHUB_REPOSITORY}`:null,workflowRunUrl:runUrl,pullRequestUrl:isGitHub&&prNumber?`${process.env.GITHUB_SERVER_URL}/${process.env.GITHUB_REPOSITORY}/pull/${prNumber}`:null,tests,artifacts};
fs.mkdirSync(path.dirname(output),{recursive:true});fs.writeFileSync(output,JSON.stringify(request,null,2));console.log(`Normalized telemetry written to ${output} (${tests.length} tests, ${artifacts.length} artifacts).`);

function featureFrom(file,fallback){const normalized=(file??'').replaceAll('\\','/');const match=normalized.match(/playwright\/(?:admin|public|visual|recording)\/([^/]+)/);return match?.[1]??fallback;}
function artifact(file,mime,correlation,feature,browserName){const normalized=path.relative(root,file).replaceAll('\\','/');const extension=path.extname(file).toLowerCase();const type=extension==='.png'?1:extension==='.webm'?2:extension==='.zip'?3:6;return{testCaseCorrelationKey:correlation,artifactType:type,provider:isGitHub?0:2,providerArtifactId:isGitHub?path.basename(file):null,name:path.basename(file),mimeType:mime??null,externalUrl:runUrl,storagePath:normalized,sizeBytes:fs.existsSync(file)?fs.statSync(file).size:null,createdAtUtc:now.toISOString(),expiresAtUtc:isGitHub?new Date(now.getTime()+14*86400000).toISOString():null,availabilityStatus:0,browser:browserName,feature};}
