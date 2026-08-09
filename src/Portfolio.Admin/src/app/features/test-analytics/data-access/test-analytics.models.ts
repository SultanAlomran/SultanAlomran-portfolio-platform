export type TestRunStatus = 0 | 1 | 2 | 3 | 4;
export type TestExecutionMode = 0 | 1 | 2;
export type TestCaseStatus = 0 | 1 | 2 | 3 | 4;
export type ArtifactAvailability = 0 | 1 | 2 | 3;

export interface TestAnalyticsQuery { from?: string; to?: string; branch?: string; status?: TestRunStatus; browser?: string; feature?: string; executionMode?: TestExecutionMode; sort?: string; page?: number; pageSize?: number; }
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; }
export interface TestRunSummary { id:string; providerRunId:string; status:TestRunStatus; branch:string; commitSha:string; pullRequestNumber:number|null; trigger:string; executionMode:TestExecutionMode; browserSummary:string; passedCount:number; failedCount:number; skippedCount:number; flakyCount:number; durationMs:number; startedAtUtc:string; artifactCount:number; workflowRunUrl:string|null; }
export interface TrendPoint { date:string; passed:number; failed:number; flaky:number; averageDurationMs:number; }
export interface BrowserStatistic { browser:string; tests:number; passed:number; failed:number; passRate:number; averageDurationMs:number; latestStatus:TestCaseStatus; }
export interface ExecutionModeStatistic { mode:TestExecutionMode; runs:number; passRate:number; latestRunAtUtc:string|null; artifacts:number; }
export interface FeatureCoverage { feature:string; tests:number; lastTestedAtUtc:string; latestStatus:TestCaseStatus; browserCoverage:number; hasVisualEvidence:boolean; hasRecording:boolean; }
export interface FlakyTest { testName:string; feature:string; browser:string; executions:number; failures:number; retries:number; flakyRate:number; lastFailedAtUtc:string|null; lastPassedAtUtc:string|null; }
export interface SlowTest { testName:string; feature:string; browser:string; averageDurationMs:number; latestDurationMs:number; }
export interface DashboardOverview { passRate:number; totalRuns:number; testsExecuted:number; failedTests:number; flakyTests:number; averageDurationMs:number; latestRun:TestRunSummary|null; browserCoverage:number; runTrend:TrendPoint[]; durationTrend:TrendPoint[]; browsers:BrowserStatistic[]; executionModes:ExecutionModeStatistic[]; features:FeatureCoverage[]; flaky:FlakyTest[]; slowest:SlowTest[]; }
export interface TestCaseResult { id:string; feature:string; suite:string; testName:string; projectArea:string; browser:string; viewport:string|null; status:TestCaseStatus; durationMs:number; retryCount:number; isFlaky:boolean; errorType:string|null; errorSummary:string|null; sourceFile:string|null; }
export interface TestArtifact { id:string; testCaseResultId:string|null; artifactType:number; provider:number; providerArtifactId:string|null; name:string; mimeType:string|null; externalUrl:string|null; storagePath:string|null; sizeBytes:number|null; createdAtUtc:string; expiresAtUtc:string|null; availabilityStatus:ArtifactAvailability; browser:string|null; feature:string|null; }
export interface TestRunDetails { run:TestRunSummary; workflowName:string; workflowRunNumber:number|null; completedAtUtc:string|null; featureSummary:string; repositoryUrl:string|null; pullRequestUrl:string|null; tests:TestCaseResult[]; artifacts:TestArtifact[]; }

export const runStatusLabel=(value:number)=>['Running','Passed','Failed','Cancelled','Timed out'][value]??'Unknown';
export const caseStatusLabel=(value:number)=>['Passed','Failed','Skipped','Timed out','Interrupted'][value]??'Unknown';
export const modeLabel=(value:number)=>['Standard','Visual','Full recording'][value]??'Unknown';
export const artifactTypeLabel=(value:number)=>['HTML report','Screenshot','Video','Trace','JUnit','JSON','Diagnostics','Other'][value]??'Other';
export const availabilityLabel=(value:number)=>['Available','Expired','Deleted','Archived'][value]??'Unavailable';
export const durationLabel=(value:number)=>value<1000?`${value} ms`:value<60000?`${(value/1000).toFixed(1)} s`:`${Math.floor(value/60000)}m ${Math.round(value%60000/1000)}s`;
