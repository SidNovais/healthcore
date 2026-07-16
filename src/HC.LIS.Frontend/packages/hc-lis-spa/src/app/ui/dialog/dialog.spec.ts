import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcDialog } from './dialog';

@Component({
  imports: [HcDialog],
  template: `
    <hc-dialog [(open)]="open" testId="print-labels-modal">
      <p>Print labels?</p>
    </hc-dialog>
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

describe('HcDialog', () => {
  // Regression (found by the first live e2e run, via hc-sheet): the testid must land on
  // the <dialog>, not the <hc-dialog> host. The host is display:inline wrapping a
  // position:fixed child, so its box is 0x0 and Playwright reports it hidden even while
  // the dialog is plainly on screen.
  it('puts the testId on the dialog panel rather than the zero-size host', () => {
    const { fixture, dialog } = render();
    const host = (fixture.nativeElement as HTMLElement).querySelector('hc-dialog')!;

    expect(dialog.getAttribute('data-testid')).toBe('print-labels-modal');
    expect(host.hasAttribute('data-testid')).toBe(false);
  });

  it('renders projected content inside a native <dialog> with the hc class', () => {
    const { dialog } = render();

    expect(dialog).not.toBeNull();
    expect(dialog.classList).toContain('hc-dialog');
    expect(dialog.textContent).toContain('Print labels?');
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
