import { Directive } from '@angular/core';

/** Styles a native <select>; CSS lives in ui/ui.css (void-adjacent native control). */
@Directive({
  selector: 'select[hc-select]',
  host: { class: 'hc-select' },
})
export class HcSelect {}
