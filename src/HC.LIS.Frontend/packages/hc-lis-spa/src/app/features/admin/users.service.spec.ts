import { TestBed } from '@angular/core/testing';
import { UsersService } from './users.service';
import { USERS_PORT, IUsersPort, CreateUserParams } from '../../core/application/i-users-port';
import type { UserSummary } from '../../core/domain/user-summary';

describe('UsersService', () => {
  let service: UsersService;
  let mockPort: IUsersPort;

  const userList: UserSummary[] = [
    {
      id: 'u-1',
      email: 'alice@hclis.local',
      fullName: 'Alice Smith',
      role: 'Receptionist',
      status: 'Active',
      createdAt: '2026-05-01T08:00:00Z',
    },
    {
      id: 'u-2',
      email: 'bob@hclis.local',
      fullName: 'Bob Jones',
      role: 'Physician',
      status: 'Active',
      createdAt: '2026-05-02T09:00:00Z',
    },
  ];

  beforeEach(() => {
    mockPort = {
      listUsers: vi.fn(),
      createUser: vi.fn(),
      changeRole: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        UsersService,
        { provide: USERS_PORT, useValue: mockPort },
      ],
    });

    service = TestBed.inject(UsersService);
  });

  it('users signal starts as empty array', () => {
    expect(service.users()).toEqual([]);
  });

  it('listUsers() calls port.listUsers and sets users signal', async () => {
    vi.mocked(mockPort.listUsers).mockResolvedValue(userList);

    await service.listUsers();

    expect(mockPort.listUsers).toHaveBeenCalledOnce();
    expect(service.users()).toEqual(userList);
  });

  it('listUsers() clears users signal when port returns empty array', async () => {
    vi.mocked(mockPort.listUsers).mockResolvedValue(userList);
    await service.listUsers();

    vi.mocked(mockPort.listUsers).mockResolvedValue([]);
    await service.listUsers();

    expect(service.users()).toEqual([]);
  });

  it('loading signal starts as false', () => {
    expect(service.loading()).toBe(false);
  });

  it('listUsers() sets loading true while the request is in flight', async () => {
    let resolve!: (users: UserSummary[]) => void;
    vi.mocked(mockPort.listUsers).mockReturnValue(new Promise((r) => { resolve = r; }));

    const pending = service.listUsers();
    expect(service.loading()).toBe(true);

    resolve(userList);
    await pending;
    expect(service.loading()).toBe(false);
  });

  it('listUsers() resets loading to false when the port rejects', async () => {
    vi.mocked(mockPort.listUsers).mockRejectedValue(new Error('boom'));

    await expect(service.listUsers()).rejects.toThrow('boom');

    expect(service.loading()).toBe(false);
  });

  it('createUser(data) calls port.createUser with supplied params', async () => {
    const params: CreateUserParams = {
      email: 'charlie@hclis.local',
      fullName: 'Charlie Brown',
      birthdate: '1990-01-15',
      gender: 'Male',
      role: 'LabTechnician',
    };
    vi.mocked(mockPort.createUser).mockResolvedValue('u-3');
    vi.mocked(mockPort.listUsers).mockResolvedValue([]);

    await service.createUser(params);

    expect(mockPort.createUser).toHaveBeenCalledWith(params);
  });

  it('changeRole(userId, role) calls port.changeRole with userId and role', async () => {
    vi.mocked(mockPort.changeRole).mockResolvedValue(undefined);
    vi.mocked(mockPort.listUsers).mockResolvedValue([]);

    await service.changeRole('u-1', 'Physician');

    expect(mockPort.changeRole).toHaveBeenCalledWith('u-1', 'Physician');
  });
});
