import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'info' | 'success' | 'error';

export interface Toast {
  id: number;
  message: string;
  variant: ToastVariant;
}

export interface ToastOptions {
  variant?: ToastVariant;
  /** Auto-dismiss delay in ms; keep within the 3-5s rule. */
  durationMs?: number;
}

const DEFAULT_DURATION_MS = 4000;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly _toasts = signal<readonly Toast[]>([]);

  readonly toasts = this._toasts.asReadonly();

  show(message: string, options: ToastOptions = {}): number {
    const id = this.nextId++;
    const toast: Toast = { id, message, variant: options.variant ?? 'info' };
    this._toasts.update(list => [...list, toast]);

    setTimeout(() => this.dismiss(id), options.durationMs ?? DEFAULT_DURATION_MS);
    return id;
  }

  dismiss(id: number): void {
    this._toasts.update(list => list.filter(t => t.id !== id));
  }
}
