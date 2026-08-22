import { Injectable, signal } from '@angular/core';

export interface LocalGuideReference {
  id: string;
  slug: string;
  title: string;
}

export interface LocalBookmark extends LocalGuideReference {
  savedAt: string;
}

export interface LocalRecentGuide extends LocalGuideReference {
  viewedAt: string;
}

export interface LocalReadingProgress extends LocalGuideReference {
  progressPercent: number;
  updatedAt: string;
}

export const visualHandbookStorageKeys = {
  bookmarks: 'portfolio.visualHandbook.bookmarks.v1',
  recentlyViewed: 'portfolio.visualHandbook.recentlyViewed.v1',
  readingProgress: 'portfolio.visualHandbook.readingProgress.v1',
} as const;

@Injectable({ providedIn: 'root' })
export class LocalEngagementService {
  private static readonly bookmarkLimit = 50;
  private static readonly recentLimit = 12;
  private static readonly progressLimit = 20;

  private readonly bookmarkState = signal(
    this.read(visualHandbookStorageKeys.bookmarks, value => this.isBookmark(value)),
  );
  private readonly recentState = signal(
    this.read(visualHandbookStorageKeys.recentlyViewed, value => this.isRecent(value)),
  );
  private readonly progressState = signal(
    this.read(visualHandbookStorageKeys.readingProgress, value => this.isProgress(value)),
  );

  readonly bookmarks = this.bookmarkState.asReadonly();
  readonly recentlyViewed = this.recentState.asReadonly();
  readonly readingProgress = this.progressState.asReadonly();

  isBookmarked(id: string): boolean {
    return this.bookmarkState().some(item => item.id === id);
  }

  toggleBookmark(guide: LocalGuideReference): boolean {
    if (this.isBookmarked(guide.id)) {
      this.removeBookmark(guide.id);
      return false;
    }
    this.saveBookmark(guide);
    return true;
  }

  saveBookmark(guide: LocalGuideReference): void {
    const value: LocalBookmark = { ...this.reference(guide), savedAt: new Date().toISOString() };
    const next = [value, ...this.bookmarkState().filter(item => item.id !== guide.id)]
      .slice(0, LocalEngagementService.bookmarkLimit);
    this.bookmarkState.set(next);
    this.write(visualHandbookStorageKeys.bookmarks, next);
  }

  removeBookmark(id: string): void {
    const next = this.bookmarkState().filter(item => item.id !== id);
    this.bookmarkState.set(next);
    this.write(visualHandbookStorageKeys.bookmarks, next);
  }

  recordViewed(guide: LocalGuideReference): void {
    const value: LocalRecentGuide = { ...this.reference(guide), viewedAt: new Date().toISOString() };
    const next = [value, ...this.recentState().filter(item => item.id !== guide.id)]
      .slice(0, LocalEngagementService.recentLimit);
    this.recentState.set(next);
    this.write(visualHandbookStorageKeys.recentlyViewed, next);
  }

  progressFor(id: string): number {
    return this.progressState().find(item => item.id === id)?.progressPercent ?? 0;
  }

  setProgress(guide: LocalGuideReference, progressPercent: number): void {
    const normalized = Math.max(0, Math.min(100, Math.round(progressPercent)));
    const existing = this.progressState().find(item => item.id === guide.id);
    if (existing && existing.progressPercent >= normalized) return;
    const value: LocalReadingProgress = {
      ...this.reference(guide),
      progressPercent: normalized,
      updatedAt: new Date().toISOString(),
    };
    const next = [value, ...this.progressState().filter(item => item.id !== guide.id)]
      .sort((left, right) => right.updatedAt.localeCompare(left.updatedAt))
      .slice(0, LocalEngagementService.progressLimit);
    this.progressState.set(next);
    this.write(visualHandbookStorageKeys.readingProgress, next);
  }

  private reference(guide: LocalGuideReference): LocalGuideReference {
    return { id: guide.id, slug: guide.slug, title: guide.title };
  }

  private read<T>(key: string, guard: (value: unknown) => value is T): T[] {
    try {
      const raw = this.storage()?.getItem(key);
      if (!raw) return [];
      const parsed: unknown = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed.filter(guard) : [];
    } catch {
      return [];
    }
  }

  private write(key: string, value: unknown): void {
    try {
      this.storage()?.setItem(key, JSON.stringify(value));
    } catch {
      // Storage may be unavailable, blocked, or full. The in-memory interaction still works.
    }
  }

  private storage(): Storage | null {
    try {
      return typeof globalThis.localStorage === 'undefined' ? null : globalThis.localStorage;
    } catch {
      return null;
    }
  }

  private isBookmark(value: unknown): value is LocalBookmark {
    return this.isReference(value) && this.hasString(value, 'savedAt') && this.isDate(value.savedAt);
  }

  private isRecent(value: unknown): value is LocalRecentGuide {
    return this.isReference(value) && this.hasString(value, 'viewedAt') && this.isDate(value.viewedAt);
  }

  private isProgress(value: unknown): value is LocalReadingProgress {
    return this.isReference(value) && this.hasString(value, 'updatedAt') && this.isDate(value.updatedAt) &&
      this.hasNumber(value, 'progressPercent') && value.progressPercent >= 0 && value.progressPercent <= 100;
  }

  private isReference(value: unknown): value is LocalGuideReference {
    return this.hasString(value, 'id') && this.hasString(value, 'slug') && this.hasString(value, 'title');
  }

  private hasString<K extends string>(value: unknown, key: K): value is Record<K, string> {
    return typeof value === 'object' && value !== null &&
      typeof (value as Record<string, unknown>)[key] === 'string';
  }

  private hasNumber<K extends string>(value: unknown, key: K): value is Record<K, number> {
    return typeof value === 'object' && value !== null &&
      typeof (value as Record<string, unknown>)[key] === 'number';
  }

  private isDate(value: string): boolean {
    return Number.isFinite(Date.parse(value));
  }
}
