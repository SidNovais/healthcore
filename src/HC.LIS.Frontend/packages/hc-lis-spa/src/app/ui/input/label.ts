import { Directive } from '@angular/core';

@Directive({
  selector: 'label[hc-label]',
  host: { class: 'hc-label' },
})
export class HcLabel {}
