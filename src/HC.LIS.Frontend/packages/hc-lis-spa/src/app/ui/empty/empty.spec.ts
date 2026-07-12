import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HcEmpty } from './empty';

@Component({
  imports: [HcEmpty],
  template: `
    <hc-empty
      title="No patients found"
      description="Try a different search."
      data-testid="patient-search-empty-state"
    >
      <button>Register patient</button>
    </hc-empty>
  `,
})
class HostComponent {}

describe('HcEmpty', () => {
  it('renders title, description and projected actions', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const empty = (fixture.nativeElement as HTMLElement).querySelector('hc-empty')!;

    expect(empty.querySelector('.hc-empty__title')!.textContent).toContain('No patients found');
    expect(empty.querySelector('.hc-empty__description')!.textContent).toContain(
      'Try a different search.',
    );
    expect(empty.querySelector('button')).not.toBeNull();
    expect(empty.getAttribute('data-testid')).toBe('patient-search-empty-state');
  });

  it('omits the description element when not provided', async () => {
    @Component({
      imports: [HcEmpty],
      template: `<hc-empty title="Nothing here" />`,
    })
    class MinimalHost {}

    const fixture = TestBed.createComponent(MinimalHost);
    fixture.detectChanges();
    const empty = (fixture.nativeElement as HTMLElement).querySelector('hc-empty')!;

    expect(empty.querySelector('.hc-empty__title')!.textContent).toContain('Nothing here');
    expect(empty.querySelector('.hc-empty__description')).toBeNull();
  });
});
