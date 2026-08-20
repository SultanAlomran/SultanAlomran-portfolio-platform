import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationsSignalRService } from '../../core/services/notifications-signalr.service';
import {
  ContactMessageDetails,
  ContactMessageStatus,
  ContactMessageSummary,
} from './data-access/message.models';
import { MessagesApiService } from './data-access/messages-api.service';

type FilterTab = 'all' | 'new' | 'read' | 'archived';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class MessagesComponent implements OnInit {
  private readonly messagesApi = inject(MessagesApiService);
  private readonly notificationsSignalR = inject(NotificationsSignalRService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly messages = signal<ContactMessageSummary[]>([]);
  readonly selectedMessage = signal<ContactMessageDetails | null>(null);
  readonly selectedId = signal<string | null>(null);
  readonly loadingList = signal<boolean>(false);
  readonly loadingDetail = signal<boolean>(false);
  readonly activeTab = signal<FilterTab>('all');
  readonly searchTerm = signal<string>('');
  readonly totalCount = signal<number>(0);
  readonly showMobileDetail = signal<boolean>(false);

  constructor() {
    effect(() => {
      const latest = this.notificationsSignalR.latestMessage();
      if (latest) {
        this.handleRealtimeIncoming();
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const targetId = params['id'];
      if (targetId) {
        this.selectedId.set(targetId);
        this.fetchMessageDetail(targetId);
        this.showMobileDetail.set(true);
      }
    });

    this.loadMessages();
  }

  setTab(tab: FilterTab): void {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.loadMessages();
  }

  onSearch(term: string): void {
    this.searchTerm.set(term);
    this.loadMessages();
  }

  loadMessages(): void {
    this.loadingList.set(true);

    let statusParam: ContactMessageStatus | undefined;
    if (this.activeTab() === 'new') statusParam = 0;
    else if (this.activeTab() === 'read') statusParam = 1;
    else if (this.activeTab() === 'archived') statusParam = 2;

    this.messagesApi
      .list({
        search: this.searchTerm(),
        status: statusParam,
        page: 1,
        pageSize: 50,
      })
      .subscribe({
        next: (result) => {
          this.messages.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loadingList.set(false);

          // If no message selected and on desktop with items, select first
          if (!this.selectedId() && result.items.length > 0 && typeof window !== 'undefined' && window.innerWidth >= 1024) {
            this.selectMessage(result.items[0]);
          }
        },
        error: () => {
          this.loadingList.set(false);
        },
      });
  }

  selectMessage(summary: ContactMessageSummary): void {
    this.selectedId.set(summary.id);
    this.showMobileDetail.set(true);
    this.fetchMessageDetail(summary.id);

    // If new, auto-mark as read
    if (summary.status === 0) {
      this.messagesApi.markAsRead(summary.id).subscribe({
        next: (updated) => {
          this.selectedMessage.set(updated);
          this.updateLocalStatus(summary.id, 1);
          this.notificationsSignalR.fetchUnreadCount();
        },
      });
    }
  }

  fetchMessageDetail(id: string): void {
    this.loadingDetail.set(true);
    this.messagesApi.getById(id).subscribe({
      next: (detail) => {
        this.selectedMessage.set(detail);
        this.loadingDetail.set(false);
      },
      error: () => {
        this.loadingDetail.set(false);
      },
    });
  }

  markAsRead(): void {
    const current = this.selectedMessage();
    if (!current || current.status === 1) return;

    this.messagesApi.markAsRead(current.id).subscribe({
      next: (updated) => {
        this.selectedMessage.set(updated);
        this.updateLocalStatus(current.id, 1);
        this.notificationsSignalR.fetchUnreadCount();
      },
    });
  }

  markAsUnread(): void {
    const current = this.selectedMessage();
    if (!current || current.status === 0) return;

    this.messagesApi.markAsUnread(current.id).subscribe({
      next: (updated) => {
        this.selectedMessage.set(updated);
        this.updateLocalStatus(current.id, 0);
        this.notificationsSignalR.fetchUnreadCount();
      },
    });
  }

  archive(): void {
    const current = this.selectedMessage();
    if (!current || current.status === 2) return;

    this.messagesApi.archive(current.id).subscribe({
      next: (updated) => {
        this.selectedMessage.set(updated);
        this.updateLocalStatus(current.id, 2);
        this.notificationsSignalR.fetchUnreadCount();
      },
    });
  }

  backToList(): void {
    this.showMobileDetail.set(false);
  }

  getReplyMailto(): string {
    const msg = this.selectedMessage();
    if (!msg) return '';
    const subject = encodeURIComponent(`Re: ${msg.subject}`);
    return `mailto:${msg.email}?subject=${subject}`;
  }

  private handleRealtimeIncoming(): void {
    // If viewing tab that includes new messages, reload
    if (this.activeTab() === 'all' || this.activeTab() === 'new') {
      this.loadMessages();
    }
  }

  private updateLocalStatus(id: string, newStatus: ContactMessageStatus): void {
    this.messages.update((list) =>
      list.map((item) => (item.id === id ? { ...item, status: newStatus } : item))
    );
  }
}
