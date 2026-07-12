// src/app/features/admin/user-list.component.ts
import { Component, OnInit, inject, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { UsersService } from './users.service';
import { CreateUserFormComponent } from './create-user-form.component';
import type { UserRole } from '../../core/domain/user-session';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcSelect } from '../../ui/select/select';
import { HcTable } from '../../ui/table/table';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CreateUserFormComponent, SlicePipe, HcBadge, HcButton, HcEmpty, HcIcon, HcSelect, HcTable],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css',
})
export class UserListComponent implements OnInit {
  protected readonly service = inject(UsersService);
  protected readonly showCreateForm = signal(false);

  ngOnInit(): void {
    void this.service.listUsers();
  }

  protected onRoleChange(userId: string, event: Event): void {
    const role = (event.target as HTMLSelectElement).value as UserRole;
    void this.service.changeRole(userId, role);
  }
}
