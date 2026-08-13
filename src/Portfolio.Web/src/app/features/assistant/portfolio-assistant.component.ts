import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';

interface AssistantSource { type: string; title: string; route: string; summary?: string }
interface AssistantResponse { message: string; sources: AssistantSource[]; actions: { type: string; label: string; route: string }[] }
interface ChatMessage { role: 'user' | 'assistant'; content: string; sources?: AssistantSource[] }

@Component({
  selector: 'app-portfolio-assistant', imports: [FormsModule, RouterLink],
  template: `
    <button class="launcher" type="button" (click)="openAssistant()" aria-label="Open Portfolio Assistant" [attr.aria-expanded]="open()">✦ <span>Ask Portfolio</span></button>
    @if (open()) {
      <div class="backdrop" (click)="close()"></div>
      <section class="panel" role="dialog" aria-modal="true" aria-labelledby="assistant-title">
        <header><div><strong id="assistant-title">Portfolio Assistant</strong><small>Ask about projects, technologies and the Visual Handbook.</small></div><button #closeButton type="button" class="icon" (click)="close()" aria-label="Close assistant">×</button></header>
        <div class="conversation" aria-live="polite">
          @if (!messages().length) {
            <div class="welcome"><span>✦</span><h2>Explore Sultan's work</h2><p>Get evidence-based answers from public portfolio content.</p></div>
            <div class="starters" aria-label="Starter questions">
              @for (starter of starters; track starter) { <button type="button" (click)="ask(starter)">{{ starter }}</button> }
            </div>
          }
          @for (item of messages(); track $index) {
            <article class="message" [class.user]="item.role === 'user'"><span class="label">{{ item.role === 'user' ? 'You' : 'Portfolio Assistant' }}</span><p>{{ item.content }}</p>
              @if (item.sources?.length) { <div class="sources"><b>Related public content</b>@for (source of item.sources; track source.route) { <a [routerLink]="source.route" (click)="close()"><small>{{ source.type }}</small><strong>{{ source.title }}</strong><span>{{ source.summary }}</span></a> }</div> }
            </article>
          }
          @if (sending()) { <div class="thinking" role="status">✦ Searching public portfolio content…</div> }
          @if (error()) { <div class="error" role="alert">{{ error() }} <button type="button" (click)="send()">Retry</button></div> }
        </div>
        <form (ngSubmit)="send()"><label for="assistant-message">Ask the portfolio</label><div><textarea id="assistant-message" name="message" [(ngModel)]="draft" maxlength="1000" rows="2" placeholder="Ask about Sultan's projects…" (keydown.control.enter)="send()"></textarea><button type="submit" [disabled]="sending() || !draft.trim()" aria-label="Send message">➤</button></div><button class="clear" type="button" (click)="clear()" [disabled]="!messages().length">Clear conversation</button></form>
      </section>
    }`,
  styleUrl: './portfolio-assistant.component.css', changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortfolioAssistantComponent {
  private readonly http = inject(HttpClient);
  readonly closeButton = viewChild<ElementRef<HTMLButtonElement>>('closeButton');
  readonly open = signal(false); readonly sending = signal(false); readonly error = signal(''); readonly messages = signal<ChatMessage[]>([]);
  draft = '';
  private lastQuestion = '';
  readonly starters = ["Show me Sultan's strongest .NET projects", 'Find Angular projects', 'What does Sultan know about OutSystems?', 'Show Visual Handbook guides about performance', "Tell me about Sultan's certifications"];
  openAssistant() { this.open.set(true); setTimeout(() => this.closeButton()?.nativeElement.focus()); }
  close() { this.open.set(false); }
  clear() { this.messages.set([]); this.error.set(''); }
  ask(question: string) { this.draft = question; this.send(); }
  send() {
    const message = this.draft.trim() || this.lastQuestion; if (!message || this.sending()) return;
    const history = this.messages().slice(-8).map(item => ({ role: item.role, content: item.content }));
    if (this.draft.trim()) this.messages.update(items => [...items, { role: 'user', content: message }]);
    this.lastQuestion = message; this.draft = ''; this.error.set(''); this.sending.set(true);
    this.http.post<AssistantResponse>(`${environment.apiUrl}/assistant/messages`, { message, conversationContext: history }).pipe(finalize(() => this.sending.set(false))).subscribe({
      next: response => this.messages.update(items => [...items, { role: 'assistant', content: response.message, sources: response.sources }]),
      error: () => this.error.set('The assistant could not respond. Please try again shortly.')
    });
  }
}
