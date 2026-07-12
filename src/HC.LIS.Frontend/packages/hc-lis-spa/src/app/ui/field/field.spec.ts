import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcField } from './field';
import { HcInput } from '../input/input';
import { HcLabel } from '../input/label';

@Component({
  imports: [HcField, HcInput, HcLabel],
  template: `
    <hc-field [helper]="helper()" [error]="error()" data-testid="email-field">
      <label hc-label for="email">Email</label>
      <input hc-input id="email" type="email" />
    </hc-field>
  `,
})
class HostComponent {
  readonly helper = signal<string | null>('We never share it.');
  readonly error = signal<string | null>(null);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const host = fixture.nativeElement as HTMLElement;
  return { fixture, host };
}

describe('HcInput / HcLabel directives', () => {
  it('apply the hc-input and hc-label classes to native elements', () => {
    const { host } = render();

    expect(host.querySelector('input')!.classList).toContain('hc-input');
    expect(host.querySelector('label')!.classList).toContain('hc-label');
  });
});

describe('HcField', () => {
  it('projects label and control content', () => {
    const { host } = render();
    const field = host.querySelector('hc-field')!;

    expect(field.querySelector('label')).not.toBeNull();
    expect(field.querySelector('input')).not.toBeNull();
  });

  it('shows the helper text when there is no error', () => {
    const { host } = render();

    const helper = host.querySelector('.hc-field__helper')!;
    expect(helper.textContent).toContain('We never share it.');
    expect(host.querySelector('.hc-field__error')).toBeNull();
  });

  it('replaces the helper with an announced error message', () => {
    const { fixture, host } = render();

    fixture.componentInstance.error.set('Email is required');
    fixture.detectChanges();

    const error = host.querySelector('.hc-field__error')!;
    expect(error.textContent).toContain('Email is required');
    expect(error.getAttribute('role')).toBe('alert');
    expect(host.querySelector('.hc-field__helper')).toBeNull();
  });
});
