import { ChangeDetectionStrategy, Component, Directive } from '@angular/core';

@Component({
  selector: 'hc-card',
  template: '<ng-content />',
  styleUrl: './card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'hc-card' },
})
export class HcCard {}

@Directive({
  selector: '[hc-card-header]',
  host: { class: 'hc-card__header' },
})
export class HcCardHeader {}

@Directive({
  selector: '[hc-card-title]',
  host: { class: 'hc-card__title' },
})
export class HcCardTitle {}

@Directive({
  selector: '[hc-card-content]',
  host: { class: 'hc-card__content' },
})
export class HcCardContent {}

@Directive({
  selector: '[hc-card-footer]',
  host: { class: 'hc-card__footer' },
})
export class HcCardFooter {}
