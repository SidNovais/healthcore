import { Injectable, inject, signal } from '@angular/core';
import { PHYSICIANS_PORT } from '../../../core/application/i-physicians-port';
import type { PhysicianSearchResult } from '../../../core/domain/physician-search-result';
import type {
  RegisterPhysicianParams,
  UpdatePhysicianParams,
} from '../../../core/domain/register-physician-params';

/**
 * The registry administers deactivated physicians as well as active ones — they are the
 * only rows the reactivate action has to work with.
 */
const IncludeInactive = true;

@Injectable({ providedIn: 'root' })
export class PhysiciansService {
  private readonly port = inject(PHYSICIANS_PORT);

  readonly physicians = signal<PhysicianSearchResult[]>([]);
  readonly loading = signal(false);

  async listPhysicians(): Promise<void> {
    this.loading.set(true);
    try {
      const physicians = await this.port.list(IncludeInactive);
      this.physicians.set(physicians);
    } finally {
      this.loading.set(false);
    }
  }

  async register(data: RegisterPhysicianParams): Promise<void> {
    await this.port.register(data);
    await this.listPhysicians();
  }

  async update(id: string, data: UpdatePhysicianParams): Promise<void> {
    await this.port.update(id, data);
    await this.listPhysicians();
  }

  async deactivate(id: string): Promise<void> {
    await this.port.deactivate(id);
    await this.listPhysicians();
  }

  async reactivate(id: string): Promise<void> {
    await this.port.reactivate(id);
    await this.listPhysicians();
  }
}
