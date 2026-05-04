import { Injectable } from '@angular/core';
import { login, me } from '@hc-lis/api-client';
import type {
  HcLisModulesUserAccessApplicationUsersLoginLoginResultDto,
  HcLisApiModulesUserAccessAuthCurrentUserMeResultDto,
} from '@hc-lis/api-client';
import type { LoginCredentials } from '../../application/i-auth-port';
import type { IAuthApi, AuthLoginResult, AuthMeResult } from './i-auth-api';

@Injectable()
export class SdkAuthApi implements IAuthApi {
  async login(credentials: LoginCredentials): Promise<AuthLoginResult> {
    const result = await login({ body: { email: credentials.email, password: credentials.password } });
    const data = result.data as HcLisModulesUserAccessApplicationUsersLoginLoginResultDto;
    return {
      userId: data?.userId ?? '',
      userEmail: data?.userEmail ?? '',
      role: data?.role ?? '',
    };
  }

  async me(): Promise<AuthMeResult> {
    const result = await me();
    const data = result.data as HcLisApiModulesUserAccessAuthCurrentUserMeResultDto;
    return {
      userId: data?.userId ?? '',
      userName: data?.userName ?? '',
      role: data?.role ?? '',
    };
  }
}
