import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, ElementRef, HostListener, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AssistantContextService } from './assistant-context.service';

interface AssistantSource { type: string; title: string; route: string; summary?: string }
interface AssistantResponse { message: string; sources: AssistantSource[]; actions: { type: string; label: string; route: string }[] }
interface ChatMessage { role: 'user' | 'assistant'; content: string; sources?: AssistantSource[] }

@Component({
  selector: 'app-portfolio-assistant',
  imports: [RouterLink],
  template: `
    <button class="launcher" type="button" (click)="openAssistant()" aria-label="Open Portfolio Assistant" [attr.aria-expanded]="isOpen()">
      ✦ <span>Ask Portfolio</span>
      @if (activeGuide(); as guide) {
        <span class="guide-indicator" title="Guide mode: {{ guide.title }}">· {{ guide.categoryName }}</span>
      }
    </button>
    @if (isOpen()) {
      <button class="backdrop" type="button" (click)="close()" aria-label="Close Portfolio Assistant"></button>
      <section class="panel" role="dialog" aria-modal="true" aria-labelledby="assistant-title">
        <header>
          <div>
            <strong id="assistant-title">Portfolio Assistant</strong>
            @if (activeGuide(); as guide) {
              <div class="guide-badge">
                <span class="mode-tag">Visual Handbook mode</span>
                <span class="guide-title-tag">Reading: {{ guide.title }}</span>
              </div>
            } @else {
              <small>Ask about projects, technologies and the Visual Handbook.</small>
            }
          </div>
          <button #closeButton type="button" class="icon" (click)="close()" aria-label="Close assistant">×</button>
        </header>
        <div class="conversation" aria-live="polite">
          @if (!messages().length) {
            @if (activeGuide(); as guide) {
              <div class="welcome">
                <span>✦</span>
                <h2>{{ guide.title }}</h2>
                <p>Ask questions about this visual guide, diagrams, or related .NET, Angular, SQL, and architecture patterns.</p>
              </div>
            } @else {
              <div class="welcome">
                <span>✦</span>
                <h2>Explore Sultan's work</h2>
                <p>Get evidence-based answers from public portfolio content.</p>
              </div>
            }
            <div class="starters" aria-label="Starter questions">
              @for (starter of currentStarters(); track starter) {
                <button type="button" (click)="ask(starter)">{{ starter }}</button>
              }
            </div>
          }
          @for (item of messages(); track $index) {
            <article class="message" [class.user]="item.role === 'user'">
              <span class="label">{{ item.role === 'user' ? 'You' : 'Portfolio Assistant' }}</span>
              <p>{{ item.content }}</p>
              @if (item.sources?.length) {
                <div class="sources">
                  <b>Related public content</b>
                  @for (source of item.sources; track source.route) {
                    <a [routerLink]="source.route" (click)="close()">
                      <small>{{ source.type }}</small>
                      <strong>{{ source.title }}</strong>
                      <span>{{ source.summary }}</span>
                    </a>
                  }
                </div>
              }
            </article>
          }
          @if (sending()) {
            <div class="thinking" role="status">✦ Searching and synthesizing context…</div>
          }
          @if (error()) {
            <div class="error" role="alert">
              {{ error() }} <button type="button" (click)="send()">Retry</button>
            </div>
          }
        </div>
        <form (submit)="$event.preventDefault(); send()">
          <label for="assistant-message">Ask the portfolio</label>
          <div>
            <textarea
              id="assistant-message"
              name="message"
              [value]="draft"
              (input)="draft = $any($event.target).value"
              maxlength="1000"
              rows="2"
              [placeholder]="placeholder()"
              (keydown.control.enter)="send()"></textarea>
            <button type="submit" [disabled]="sending() || !draft.trim()" aria-label="Send message">➤</button>
          </div>
          <button class="clear" type="button" (click)="clear()" [disabled]="!messages().length">Clear conversation</button>
        </form>
      </section>
    }`,
  styleUrl: './portfolio-assistant.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PortfolioAssistantComponent {
  private readonly http = inject(HttpClient);
  readonly context = inject(AssistantContextService);
  readonly closeButton = viewChild<ElementRef<HTMLButtonElement>>('closeButton');
  readonly sending = signal(false);
  readonly error = signal('');
  readonly messages = signal<ChatMessage[]>([]);
  draft = '';
  private lastQuestion = '';

  readonly activeGuide = this.context.activeGuide;
  readonly isOpen = this.context.isOpen;

  readonly defaultStarters = [
    "Show me Sultan's strongest .NET projects",
    'Find Angular projects',
    'What does Sultan know about OutSystems?',
    'Show Visual Handbook guides about performance',
    "Tell me about Sultan's certifications"
  ];

  readonly guideStarters = computed(() => {
    const guide = this.activeGuide();
    if (!guide) return this.defaultStarters;
    return [
      `Summarize ${guide.title}`,
      'Explain this guide in simple terms',
      'Give me a real-world production example',
      'When should I use this pattern vs alternatives?',
      'Show me a code example for this',
      'Quiz me on the concepts in this guide'
    ];
  });

  readonly currentStarters = computed(() =>
    this.activeGuide() ? this.guideStarters() : this.defaultStarters);

  readonly placeholder = computed(() =>
    this.activeGuide()
      ? `Ask about ${this.activeGuide()?.title}…`
      : "Ask about Sultan's projects…");

  constructor() {
    effect(() => {
      const requested = this.context.requestedPrompt();
      if (requested) {
        this.context.clearPrompt();
        this.ask(requested);
      }
    });

    effect(() => {
      if (this.isOpen()) {
        setTimeout(() => this.closeButton()?.nativeElement.focus());
      }
    });
  }

  openAssistant() {
    this.context.open();
  }

  close() {
    this.context.close();
  }

  clear() {
    this.messages.set([]);
    this.error.set('');
    this.lastQuestion = '';
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    if (this.isOpen()) this.close();
  }

  ask(question: string) {
    this.draft = question;
    this.send();
  }

  send() {
    const message = this.draft.trim() || this.lastQuestion;
    if (!message || this.sending()) return;

    const history = this.messages().slice(-8).map(item => ({ role: item.role, content: item.content }));
    if (this.draft.trim()) this.messages.update(items => [...items, { role: 'user', content: message }]);
    this.lastQuestion = message;
    this.draft = '';
    this.error.set('');
    this.sending.set(true);

    const payload: { message: string; conversationContext: typeof history; guideSlug?: string } = {
      message,
      conversationContext: history,
      ...(this.activeGuide()?.slug ? { guideSlug: this.activeGuide()!.slug } : {})
    };

    this.http.post<AssistantResponse>(`${environment.apiUrl}/assistant/messages`, payload)
      .pipe(finalize(() => this.sending.set(false)))
      .subscribe({
        next: response => this.messages.update(items => [...items, { role: 'assistant', content: response.message, sources: response.sources }]),
        error: () => this.error.set('The assistant could not respond. Please try again shortly.')
      });
  }
}

