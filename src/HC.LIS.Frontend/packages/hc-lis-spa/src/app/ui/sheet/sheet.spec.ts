import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcSheet } from './sheet';

@Component({
  imports: [HcSheet],
  template: `
    <hc-sheet [(open)]="open" ariaLabel="Patient detail" data-testid="patient-detail-sheet">
      <p>Sheet body</p>
    </hc-sheet>
  `,
})
class HostComponent {
  readonly open = signal(false);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const dialog = (fixture.nativeElement as HTMLElement).querySelector('dialog')!;
  return { fixture, dialog };
}

describe('HcSheet', () => {
  it('renders projected content inside a native <dialog> with the hc-sheet class', () => {
    const { dialog } = render();

    expect(dialog).not.toBeNull();
    expect(dialog.classList).toContain('hc-sheet');
    expect(dialog.textContent).toContain('Sheet body');
  });

  it('exposes the aria-label on the dialog panel', () => {
    const { dialog } = render();

    expect(dialog.getAttribute('aria-label')).toBe('Patient detail');
  });

  it('opens as a modal when the open model becomes true', async () => {
    const { fixture, dialog } = render();

    expect(dialog.hasAttribute('open')).toBe(false);

    fixture.componentInstance.open.set(true);
    await fixture.whenStable();

    expect(dialog.hasAttribute('open')).toBe(true);
  });

  it('closes when the open model becomes false again', async () => {
    const { fixture, dialog } = render();

    fixture.componentInstance.open.set(true);
    await fixture.whenStable();
    fixture.componentInstance.open.set(false);
    await fixture.whenStable();

    expect(dialog.hasAttribute('open')).toBe(false);
  });

  it('syncs the model back when the native dialog closes (Esc key path)', async () => {
    const { fixture, dialog } = render();

    fixture.componentInstance.open.set(true);
    await fixture.whenStable();

    // The test DOM cannot simulate the UA Esc behavior; emulate its outcome
    dialog.removeAttribute('open');
    dialog.dispatchEvent(new Event('close'));
    await fixture.whenStable();

    expect(fixture.componentInstance.open()).toBe(false);
  });
});
