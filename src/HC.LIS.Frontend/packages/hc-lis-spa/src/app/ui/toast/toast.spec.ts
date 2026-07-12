import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { ToastService } from './toast.service';
import { HcToaster } from './toaster';

describe('ToastService', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('show() adds a toast with message and variant', () => {
    const service = TestBed.inject(ToastService);

    service.show('Report signed', { variant: 'success' });

    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0].message).toBe('Report signed');
    expect(service.toasts()[0].variant).toBe('success');
  });

  it('auto-dismisses after the duration (default 4s, within the 3-5s rule)', () => {
    const service = TestBed.inject(ToastService);

    service.show('Saved');
    expect(service.toasts()).toHaveLength(1);

    vi.advanceTimersByTime(4000);

    expect(service.toasts()).toHaveLength(0);
  });

  it('dismiss(id) removes a toast immediately', () => {
    const service = TestBed.inject(ToastService);

    const id = service.show('Saved');
    service.dismiss(id);

    expect(service.toasts()).toHaveLength(0);
  });
});

describe('HcToaster', () => {
  it('renders active toasts inside a polite live region', () => {
    const fixture = TestBed.createComponent(HcToaster);
    const service = TestBed.inject(ToastService);
    fixture.detectChanges();

    service.show('Order created', { variant: 'success' });
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const region = host.querySelector('.hc-toaster')!;
    expect(region.getAttribute('aria-live')).toBe('polite');

    const toast = host.querySelector('[data-testid="toast"]')!;
    expect(toast.textContent).toContain('Order created');
    expect(toast.classList).toContain('hc-toast--success');
  });
});
