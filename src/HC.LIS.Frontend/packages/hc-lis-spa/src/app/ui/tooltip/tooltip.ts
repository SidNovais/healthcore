import { DestroyRef, Directive, ElementRef, inject, input } from '@angular/core';

let tooltipSeq = 0;

/** Text tooltip for icon-only controls; shown on hover/focus, described via ARIA. */
@Directive({
  selector: '[hcTooltip]',
  host: {
    '(mouseenter)': 'show()',
    '(mouseleave)': 'hide()',
    '(focus)': 'show()',
    '(blur)': 'hide()',
  },
})
export class HcTooltip {
  readonly hcTooltip = input.required<string>();

  private readonly host = inject(ElementRef).nativeElement as HTMLElement;
  private tooltipEl: HTMLElement | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.hide());
  }

  protected show(): void {
    if (this.tooltipEl || !this.hcTooltip()) {
      return;
    }
    const el = document.createElement('div');
    el.className = 'hc-tooltip';
    el.id = `hc-tooltip-${++tooltipSeq}`;
    el.setAttribute('role', 'tooltip');
    el.textContent = this.hcTooltip();
    document.body.appendChild(el);

    const rect = this.host.getBoundingClientRect();
    el.style.left = `${rect.left + rect.width / 2}px`;
    el.style.top = `${rect.top - 8}px`;

    this.host.setAttribute('aria-describedby', el.id);
    this.tooltipEl = el;
  }

  protected hide(): void {
    this.tooltipEl?.remove();
    this.tooltipEl = null;
    this.host.removeAttribute('aria-describedby');
  }
}
