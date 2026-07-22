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

@Component({
  imports: [HcDialog],
  template: `
    <hc-dialog [(open)]="open" width="wide" testId="create-user-dialog">
      <p>Print labels?</p>
    </hc-dialog>
  `,
})
class WideHostComponent {
  readonly open = signal(false);
}

function render() {
  const fixture = TestBed.createComponent(HostComponent);
  fixture.detectChanges();
  const dialog = (fixture.nativeElement as HTMLElement).querySelector('dialog')!;
  return { fixture, dialog };
}

function renderWide() {
  const fixture = TestBed.createComponent(WideHostComponent);
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

  // The width vocabulary exists so form-heavy dialogs stop re-declaring a private
  // max-width on their projected content (create-user's form was capped at 420px inside
  // a 32rem panel, which is what starved its fields).
  it('caps the panel at the narrow measure by default', () => {
    const { dialog } = render();

    expect(dialog.classList).toContain('hc-dialog--narrow');
    expect(dialog.classList).not.toContain('hc-dialog--wide');
  });

  it('applies the wide measure when width is "wide"', () => {
    const { dialog } = renderWide();

    expect(dialog.classList).toContain('hc-dialog--wide');
    expect(dialog.classList).not.toContain('hc-dialog--narrow');
    // The base class still has to survive the modifier binding
    expect(dialog.classList).toContain('hc-dialog');
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

  // Light-dismiss: a click on the backdrop closes the modal. A click on the <dialog>
  // element itself is a backdrop click — projected content lives in an inner wrapper, so
  // content clicks target that wrapper (or deeper) and never the <dialog>. The press must
  // also START on the backdrop, so selecting text inside a field and releasing past the
  // panel edge does not dismiss.
  describe('backdrop dismiss', () => {
    async function opened() {
      const h = render();
      h.fixture.componentInstance.open.set(true);
      await h.fixture.whenStable();
      const content = h.dialog.querySelector('p')!;
      return { ...h, content };
    }

    it('closes when a press starts and ends on the backdrop', async () => {
      const { fixture, dialog } = await opened();

      dialog.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
      dialog.dispatchEvent(new MouseEvent('click', { bubbles: true }));

      expect(fixture.componentInstance.open()).toBe(false);
    });

    it('stays open when the click lands on projected content', async () => {
      const { fixture, content } = await opened();

      content.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
      content.dispatchEvent(new MouseEvent('click', { bubbles: true }));

      expect(fixture.componentInstance.open()).toBe(true);
    });

    it('stays open when a press starts inside and releases on the backdrop', async () => {
      const { fixture, dialog, content } = await opened();

      content.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
      dialog.dispatchEvent(new MouseEvent('click', { bubbles: true }));

      expect(fixture.componentInstance.open()).toBe(true);
    });
  });
});
