import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'info' | 'success' | 'error';

export interface Toast {
  id: number;
  message: string;
  variant: ToastVariant;
  /** Optional stable data-testid so a specific toast can be targeted in e2e. */
  testId: string | null;
}

export interface ToastOptions {
  variant?: ToastVariant;
  /** Auto-dismiss delay in ms; keep within the 3-5s rule. */
  durationMs?: number;
  /** Stable data-testid; a new toast with the same testId replaces the previous one. */
  testId?: string;
}

const DEFAULT_DURATION_MS = 4000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly _toasts = signal<readonly Toast[]>([]);

  readonly toasts = this._toasts.asReadonly();

  show(message: string, options: ToastOptions = {}): number {
    const id = this.nextId++;
    const testId = options.testId ?? null;
    const toast: Toast = { id, message, variant: options.variant ?? 'info', testId };
    // Dedupe by testId: a fresh confirmation of the same kind replaces the prior one.
    this._toasts.update(list => [
      ...(testId === null ? list : list.filter(t => t.testId !== testId)),
      toast,
    ]);

    setTimeout(() => this.dismiss(id), options.durationMs ?? DEFAULT_DURATION_MS);
    return id;
  }

  dismiss(id: number): void {
    this._toasts.update(list => list.filter(t => t.id !== id));
  }
}
