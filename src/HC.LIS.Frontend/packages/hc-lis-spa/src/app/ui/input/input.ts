import { Directive } from '@angular/core';

/**
 * Styles a native input/textarea with the design-system look.
 * <input> is a void element, so this is a directive; its CSS lives in ui/ui.css.
 */
@Directive({
  selector: 'input[hc-input], textarea[hc-input]',
  host: { class: 'hc-input' },
})
export class HcInput {}
