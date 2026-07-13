import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { UserListComponent } from './user-list.component';
import { UsersService } from './users.service';
import type { UserSummary } from '../../core/domain/user-summary';

describe('UserListComponent (integration)', () => {
  let fixture: ComponentFixture<UserListComponent>;
  let mockService: Partial<UsersService>;
  let usersSignal: ReturnType<typeof signal<UserSummary[]>>;
  let loadingSignal: ReturnType<typeof signal<boolean>>;

  const twoUsers: UserSummary[] = [
    { id: 'u-1', email: 'alice@hclis.local', fullName: 'Alice Smith', role: 'Receptionist', status: 'Active', createdAt: '2026-05-01T08:00:00Z' },
    { id: 'u-2', email: 'bob@hclis.local', fullName: 'Bob Jones', role: 'Physician', status: 'Active', createdAt: '2026-05-02T09:00:00Z' },
  ];

  function makeUsers(n: number): UserSummary[] {
    return Array.from({ length: n }, (_, i) => ({
      id: `u-${i + 1}`,
      email: `user${i + 1}@hclis.local`,
      fullName: `User ${i + 1}`,
      role: 'Receptionist',
      status: 'Active',
      createdAt: '2026-05-01T08:00:00Z',
    }));
  }

  function rows(): HTMLElement[] {
    return Array.from(host().querySelectorAll('[data-testid="user-row"]'));
  }

  beforeEach(async () => {
    usersSignal = signal<UserSummary[]>([]);
    loadingSignal = signal(false);

    mockService = {
      users: usersSignal,
      loading: loadingSignal,
      listUsers: vi.fn().mockResolvedValue(undefined),
      createUser: vi.fn().mockResolvedValue(undefined),
      changeRole: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [UserListComponent],
      providers: [{ provide: UsersService, useValue: mockService }],
    }).compileComponents();

    fixture = TestBed.createComponent(UserListComponent);
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  function host(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  it('shows skeleton rows while the list is loading', () => {
    loadingSignal.set(true);
    fixture.detectChanges();

    const skeletons = host().querySelectorAll('[data-testid="users-skeleton-row"]');
    expect(skeletons.length).toBeGreaterThan(0);
    expect(host().querySelectorAll('[data-testid="user-row"]')).toHaveLength(0);
    expect(host().querySelector('[data-testid="empty-state"]')).toBeNull();
  });

  it('does not show skeleton rows once loading completes with data', () => {
    loadingSignal.set(false);
    usersSignal.set(twoUsers);
    fixture.detectChanges();

    expect(host().querySelectorAll('[data-testid="users-skeleton-row"]')).toHaveLength(0);
    expect(host().querySelectorAll('[data-testid="user-row"]')).toHaveLength(2);
  });

  it('shows the empty-state (not skeletons) once loading completes with no data', () => {
    loadingSignal.set(false);
    usersSignal.set([]);
    fixture.detectChanges();

    expect(host().querySelectorAll('[data-testid="users-skeleton-row"]')).toHaveLength(0);
    expect(host().querySelector('[data-testid="empty-state"]')).not.toBeNull();
  });

  it('paginates the user list, one page at a time', () => {
    usersSignal.set(makeUsers(23));
    fixture.detectChanges();

    expect(rows()).toHaveLength(10);
    expect(host().querySelector('[data-testid="users-pagination"]')).not.toBeNull();

    host().querySelector<HTMLButtonElement>('[data-testid="users-pagination-next"]')!.click();
    fixture.detectChanges();
    expect(rows()).toHaveLength(10);
    expect(rows()[0].textContent).toContain('user11@hclis.local');
  });

  it('hides pagination when a single page of users fits', () => {
    usersSignal.set(twoUsers);
    fixture.detectChanges();

    expect(host().querySelector('[data-testid="users-pagination"]')).toBeNull();
  });

  it('changes a role via the menu after confirming in a dialog', async () => {
    usersSignal.set(twoUsers);
    fixture.detectChanges();

    host().querySelector<HTMLButtonElement>('[data-testid="user-role-trigger"]')!.click();
    fixture.detectChanges();
    host().querySelector<HTMLButtonElement>('[data-testid="user-role-option-Physician"]')!.click();
    fixture.detectChanges();

    // The confirm dialog names the pending change and has not yet applied it.
    const dialog = host().querySelector('[data-testid="role-change-dialog"]')!;
    expect(dialog.textContent).toContain('alice@hclis.local');
    expect(dialog.textContent).toContain('Physician');
    expect(mockService.changeRole).not.toHaveBeenCalled();

    host().querySelector<HTMLButtonElement>('[data-testid="confirm-role-change-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.changeRole).toHaveBeenCalledWith('u-1', 'Physician');
  });

  it('does not change a role when the confirm dialog is cancelled', async () => {
    usersSignal.set(twoUsers);
    fixture.detectChanges();

    host().querySelector<HTMLButtonElement>('[data-testid="user-role-trigger"]')!.click();
    fixture.detectChanges();
    host().querySelector<HTMLButtonElement>('[data-testid="user-role-option-Physician"]')!.click();
    fixture.detectChanges();

    host().querySelector<HTMLButtonElement>('[data-testid="cancel-role-change-btn"]')!.click();
    await fixture.whenStable();

    expect(mockService.changeRole).not.toHaveBeenCalled();
  });

  it('opens the create-user form inside a dialog', async () => {
    host().querySelector<HTMLButtonElement>('[data-testid="create-user-btn"]')!.click();
    fixture.detectChanges();

    // Form is rendered inside the dialog wrapper (native modal open state is a UA concern).
    const dialog = host().querySelector('[data-testid="create-user-dialog"]')!;
    expect(dialog.querySelector('[data-testid="create-user-form"]')).not.toBeNull();
  });
});
