import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { AUTH_PORT, IAuthPort, LoginCredentials } from './i-auth-port';
import type { UserSession } from '../domain/user-session';

describe('AuthService', () => {
  let service: AuthService;
  let mockPort: IAuthPort;

  const receptionist: UserSession = { userId: '1', userName: 'rcpt@hclis.local', role: 'Receptionist' };
  const creds: LoginCredentials = { email: 'rcpt@hclis.local', password: 'pass123' };

  beforeEach(() => {
    mockPort = { login: vi.fn(), me: vi.fn(), logout: vi.fn() };

    TestBed.configureTestingModule({
      providers: [AuthService, { provide: AUTH_PORT, useValue: mockPort }],
    });

    service = TestBed.inject(AuthService);
  });

  it('currentUser signal starts as null', () => {
    expect(service.currentUser()).toBeNull();
  });

  it('login() calls port.login with credentials and sets currentUser on success', async () => {
    vi.mocked(mockPort.login).mockResolvedValue(receptionist);

    await service.login(creds);

    expect(mockPort.login).toHaveBeenCalledWith(creds);
    expect(service.currentUser()).toEqual(receptionist);
  });

  it('login() propagates error without setting signal', async () => {
    vi.mocked(mockPort.login).mockRejectedValue(new Error('bad credentials'));

    await expect(service.login(creds)).rejects.toThrow('bad credentials');
    expect(service.currentUser()).toBeNull();
  });

  it('me() sets currentUser from port response', async () => {
    vi.mocked(mockPort.me).mockResolvedValue(receptionist);

    await service.me();

    expect(service.currentUser()).toEqual(receptionist);
  });

  it('me() sets currentUser to null if port throws', async () => {
    vi.mocked(mockPort.me).mockRejectedValue(new Error('401'));

    await service.me();

    expect(service.currentUser()).toBeNull();
  });

  it('logout() calls port.logout and resets currentUser to null', async () => {
    vi.mocked(mockPort.login).mockResolvedValue(receptionist);
    await service.login(creds);
    expect(service.currentUser()).not.toBeNull();

    vi.mocked(mockPort.logout).mockResolvedValue();

    await service.logout();

    expect(mockPort.logout).toHaveBeenCalledOnce();
    expect(service.currentUser()).toBeNull();
  });
});
