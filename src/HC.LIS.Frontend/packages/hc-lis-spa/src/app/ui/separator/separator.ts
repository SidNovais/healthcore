import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'hc-separator',
  template: '',
  styleUrl: './separator.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'separator',
    '[attr.aria-orientation]': 'orientation()',
    '[class]': 'classes()',
  },
})
export class HcSeparator {
  readonly orientation = input<'horizontal' | 'vertical'>('horizontal');

  protected readonly classes = computed(
    () => `hc-separator${this.orientation() === 'vertical' ? ' hc-separator--vertical' : ''}`,
  );
}
