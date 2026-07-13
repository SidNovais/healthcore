// src/app/features/admin/user-list.component.ts
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { UsersService } from './users.service';
import { CreateUserFormComponent } from './create-user-form.component';
import type { UserSummary } from '../../core/domain/user-summary';
import type { UserRole } from '../../core/domain/user-session';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcDialog } from '../../ui/dialog/dialog';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcPagination } from '../../ui/pagination/pagination';
import { HcSkeleton } from '../../ui/skeleton/skeleton';
import { HcTable } from '../../ui/table/table';
import { ToastService } from '../../ui/toast/toast.service';

interface RoleOption {
  value: UserRole;
  label: string;
}

interface PendingRoleChange {
  userId: string;
  email: string;
  currentRole: UserRole;
  newRole: UserRole;
}

const PAGE_SIZE = 10;

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CreateUserFormComponent,
    SlicePipe,
    HcBadge,
    HcButton,
    HcDialog,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcEmpty,
    HcIcon,
    HcPagination,
    HcSkeleton,
    HcTable,
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css',
})
export class UserListComponent implements OnInit {
  protected readonly service = inject(UsersService);
  private readonly toast = inject(ToastService);

  protected readonly showCreateForm = signal(false);
  protected readonly skeletonRows = Array.from({ length: 5 });
  protected readonly page = signal(1);

  protected readonly roles: readonly RoleOption[] = [
    { value: 'Receptionist', label: 'Receptionist' },
    { value: 'LabTechnician', label: 'Lab Technician' },
    { value: 'Physician', label: 'Physician' },
    { value: 'ITAdmin', label: 'IT Admin' },
  ];

  protected readonly roleDialogOpen = signal(false);
  protected readonly pendingRoleChange = signal<PendingRoleChange | null>(null);

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.service.users().length / PAGE_SIZE)),
  );

  protected readonly displayPage = computed(() => Math.min(this.page(), this.pageCount()));

  protected readonly pagedUsers = computed<UserSummary[]>(() => {
    const start = (this.displayPage() - 1) * PAGE_SIZE;
    return this.service.users().slice(start, start + PAGE_SIZE);
  });

  ngOnInit(): void {
    void this.service.listUsers();
  }

  protected roleLabel(role: UserRole): string {
    return this.roles.find(r => r.value === role)?.label ?? role;
  }

  protected requestRoleChange(user: UserSummary, newRole: UserRole): void {
    if (user.role === newRole) {
      return;
    }
    this.pendingRoleChange.set({
      userId: user.id,
      email: user.email,
      currentRole: user.role,
      newRole,
    });
    this.roleDialogOpen.set(true);
  }

  protected async confirmRoleChange(): Promise<void> {
    const pending = this.pendingRoleChange();
    this.roleDialogOpen.set(false);
    if (pending === null) {
      return;
    }
    this.pendingRoleChange.set(null);
    await this.service.changeRole(pending.userId, pending.newRole);
    this.toast.show(`Role updated to ${this.roleLabel(pending.newRole)}.`, {
      variant: 'success',
      testId: 'role-change-toast',
    });
  }

  protected cancelRoleChange(): void {
    this.roleDialogOpen.set(false);
    this.pendingRoleChange.set(null);
  }
}
