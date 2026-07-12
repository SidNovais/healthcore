// src/app/features/admin/user-list.component.ts
import { Component, OnInit, inject, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { UsersService } from './users.service';
import { CreateUserFormComponent } from './create-user-form.component';
import type { UserRole } from '../../core/domain/user-session';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CreateUserFormComponent, SlicePipe],
  template: `
    <div class="users-page">
      <div class="users-header">
        <h2 data-testid="users-title">User Management</h2>
        <button data-testid="create-user-btn" (click)="showCreateForm.set(true)">
          + Create User
        </button>
      </div>

      @if (showCreateForm()) {
        <div class="form-wrapper">
          <app-create-user-form (close)="showCreateForm.set(false)" />
        </div>
      }

      @if (service.users().length === 0) {
        <p data-testid="empty-state" class="empty-state">No users found.</p>
      } @else {
        <table class="users-table">
          <thead>
            <tr>
              <th>Email</th>
              <th>Full Name</th>
              <th>Role</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            @for (user of service.users(); track user.id) {
              <tr data-testid="user-row">
                <td>{{ user.email }}</td>
                <td>{{ user.fullName }}</td>
                <td>
                  <select
                    [value]="user.role"
                    [attr.aria-label]="'Role for ' + user.email"
                    (change)="onRoleChange(user.id, $event)"
                  >
                    <option value="Receptionist">Receptionist</option>
                    <option value="LabTechnician">Lab Technician</option>
                    <option value="Physician">Physician</option>
                    <option value="ITAdmin">IT Admin</option>
                  </select>
                </td>
                <td>{{ user.status }}</td>
                <td>{{ user.createdAt | slice: 0 : 10 }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    .users-page { padding: 2rem; }
    .users-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .users-header h2 { margin: 0; }
    .users-header button { padding: 0.4rem 0.9rem; background: var(--color-accent); color: var(--color-surface); border: none; border-radius: 4px; cursor: pointer; }
    .form-wrapper { margin-bottom: 1.5rem; }
    .empty-state { color: var(--color-text-muted, #888); }
    .users-table { width: 100%; border-collapse: collapse; }
    .users-table th, .users-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid #e0e0e0; text-align: left; }
    .users-table select { border: 1px solid #ccc; border-radius: 4px; padding: 0.3rem; cursor: pointer; }
  `],
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
