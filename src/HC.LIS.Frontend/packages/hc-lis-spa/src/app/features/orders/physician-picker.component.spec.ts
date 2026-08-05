import { TestBed } from '@angular/core/testing';
import type { ComponentFixture } from '@angular/core/testing';
import { PhysicianPickerComponent } from './physician-picker.component';
import { PHYSICIANS_PORT } from '../../core/application/i-physicians-port';
import type { PhysicianSearchResult } from '../../core/domain/physician-search-result';

const ANA: PhysicianSearchResult = {
  id: 'physician-uuid-1',
  fullName: 'Ana Lima',
  licenceNumber: 'CRM-12345',
  status: 'Active',
};

const RETIRED: PhysicianSearchResult = {
  id: 'physician-uuid-2',
  fullName: 'Ana Retired',
  licenceNumber: null,
  status: 'Inactive',
};

describe('PhysicianPickerComponent', () => {
  let port: {
    search: ReturnType<typeof vi.fn>;
    list: ReturnType<typeof vi.fn>;
    getDetails: ReturnType<typeof vi.fn>;
    register: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    deactivate: ReturnType<typeof vi.fn>;
    reactivate: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    vi.useFakeTimers();

    port = {
      search: vi.fn().mockResolvedValue([]),
      list: vi.fn().mockResolvedValue([]),
      getDetails: vi.fn(),
      register: vi.fn(),
      update: vi.fn(),
      deactivate: vi.fn(),
      reactivate: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [PhysicianPickerComponent],
      providers: [{ provide: PHYSICIANS_PORT, useValue: port }],
    }).compileComponents();
  });

  afterEach(() => {
    vi.useRealTimers();
    TestBed.resetTestingModule();
  });

  function render() {
    const fixture = TestBed.createComponent(PhysicianPickerComponent);
    const emitted: (PhysicianSearchResult | null)[] = [];
    fixture.componentInstance.physicianSelected.subscribe(value => emitted.push(value));
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement, emitted };
  }

  function query<T extends HTMLElement>(el: HTMLElement, testId: string): T | null {
    return el.querySelector<T>(`[data-testid="${testId}"]`);
  }

  function queryAll(el: HTMLElement, testId: string): HTMLElement[] {
    return Array.from(el.querySelectorAll<HTMLElement>(`[data-testid="${testId}"]`));
  }

  async function search(fixture: ComponentFixture<PhysicianPickerComponent>, el: HTMLElement, term: string) {
    const input = query<HTMLInputElement>(el, 'physician-picker-input')!;
    input.value = term;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(300);
    fixture.detectChanges();
  }

  function type(el: HTMLElement, testId: string, value: string): void {
    const input = query<HTMLInputElement>(el, testId)!;
    input.value = value;
    input.dispatchEvent(new Event('input', { bubbles: true }));
  }

  it('does not search until the debounce elapses', async () => {
    const { fixture, el } = render();

    const input = query<HTMLInputElement>(el, 'physician-picker-input')!;
    input.value = 'Ana';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    fixture.detectChanges();

    expect(port.search).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(300);
    expect(port.search).toHaveBeenCalledWith('Ana');
  });

  it('lists active physicians and filters out inactive ones', async () => {
    port.search.mockResolvedValue([ANA, RETIRED]);
    const { fixture, el } = render();

    await search(fixture, el, 'Ana');

    const items = queryAll(el, 'physician-picker-result-item');
    expect(items).toHaveLength(1);
    expect(items[0].textContent).toContain('Ana Lima');
    expect(items[0].textContent).toContain('CRM-12345');
  });

  it('selecting a result emits it and shows the selected card', async () => {
    port.search.mockResolvedValue([ANA]);
    const { fixture, el, emitted } = render();

    await search(fixture, el, 'Ana');
    query(el, 'physician-picker-result-item')!.click();
    fixture.detectChanges();

    expect(emitted).toEqual([ANA]);
    const card = query(el, 'physician-picker-selected-card')!;
    expect(card.textContent).toContain('Ana Lima');
    expect(query(el, 'physician-picker-input')).toBeNull();
  });

  it('clearing the selection emits null and restores the search input', async () => {
    port.search.mockResolvedValue([ANA]);
    const { fixture, el, emitted } = render();

    await search(fixture, el, 'Ana');
    query(el, 'physician-picker-result-item')!.click();
    fixture.detectChanges();

    query(el, 'physician-picker-clear-btn')!.click();
    fixture.detectChanges();

    expect(emitted).toEqual([ANA, null]);
    expect(query(el, 'physician-picker-selected-card')).toBeNull();
    expect(query(el, 'physician-picker-input')).not.toBeNull();
  });

  it('offers quick-add only once a search has come back empty', async () => {
    const { fixture, el } = render();

    expect(query(el, 'physician-picker-quick-add-btn')).toBeNull();

    await search(fixture, el, 'Nobody');

    expect(query(el, 'physician-picker-quick-add-btn')).not.toBeNull();
  });

  it('quick-add registers the physician and selects it immediately', async () => {
    port.register.mockResolvedValue('physician-uuid-new');
    const { fixture, el, emitted } = render();

    await search(fixture, el, 'Bruno');
    query(el, 'physician-picker-quick-add-btn')!.click();
    fixture.detectChanges();

    type(el, 'physician-quick-add-name-input', 'Bruno Costa');
    type(el, 'physician-quick-add-licence-input', 'CRM-999');
    fixture.detectChanges();

    query(el, 'physician-quick-add-submit-btn')!.click();
    await vi.advanceTimersByTimeAsync(0);
    fixture.detectChanges();

    expect(port.register).toHaveBeenCalledWith({
      fullName: 'Bruno Costa',
      licenceNumber: 'CRM-999',
    });
    expect(emitted).toEqual([
      {
        id: 'physician-uuid-new',
        fullName: 'Bruno Costa',
        licenceNumber: 'CRM-999',
        status: 'Active',
      },
    ]);
    expect(query(el, 'physician-picker-selected-card')!.textContent).toContain('Bruno Costa');
    expect(query(el, 'physician-quick-add-name-input')).toBeNull();
  });

  it('quick-add sends no licence number when the field is left blank', async () => {
    port.register.mockResolvedValue('physician-uuid-new');
    const { fixture, el, emitted } = render();

    await search(fixture, el, 'Bruno');
    query(el, 'physician-picker-quick-add-btn')!.click();
    fixture.detectChanges();

    type(el, 'physician-quick-add-name-input', 'Bruno Costa');
    fixture.detectChanges();

    query(el, 'physician-quick-add-submit-btn')!.click();
    await vi.advanceTimersByTimeAsync(0);
    fixture.detectChanges();

    expect(port.register).toHaveBeenCalledWith({ fullName: 'Bruno Costa', licenceNumber: undefined });
    expect(emitted[0]).toMatchObject({ licenceNumber: null });
  });

  it('surfaces a quick-add failure and keeps the dialog open', async () => {
    port.register.mockRejectedValue(new Error('Physician must have a full name'));
    const { fixture, el, emitted } = render();

    await search(fixture, el, 'Bruno');
    query(el, 'physician-picker-quick-add-btn')!.click();
    fixture.detectChanges();

    type(el, 'physician-quick-add-name-input', 'Bruno Costa');
    fixture.detectChanges();

    query(el, 'physician-quick-add-submit-btn')!.click();
    await vi.advanceTimersByTimeAsync(0);
    fixture.detectChanges();

    expect(emitted).toEqual([]);
    expect(query(el, 'physician-quick-add-name-input')).not.toBeNull();
    expect(query(el, 'physician-quick-add-error')!.textContent).toContain(
      'Physician must have a full name',
    );
  });
});
