import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcSelect } from './select';

@Component({
  imports: [HcSelect],
  template: `
    <select hc-select data-testid="role-select">
      <option value="a">A</option>
    </select>
  `,
})
class HostComponent {}

describe('HcSelect', () => {
  it('applies the hc-select class to the native select and keeps the testid', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const select = (fixture.nativeElement as HTMLElement).querySelector('select')!;

    expect(select.classList).toContain('hc-select');
    expect(select.getAttribute('data-testid')).toBe('role-select');
  });
});
