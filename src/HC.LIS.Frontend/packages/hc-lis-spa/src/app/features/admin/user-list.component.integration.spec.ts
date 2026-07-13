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

  beforeEach(async () => {
    usersSignal = signal<UserSummary[]>([]);
    loadingSignal = signal(false);

    mockService = {
      users: usersSignal,
      loading: loadingSignal,
      listUsers: vi.fn().mockResolvedValue(undefined),
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
});
