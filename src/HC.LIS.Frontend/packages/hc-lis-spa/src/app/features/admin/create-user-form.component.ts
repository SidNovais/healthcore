// src/app/features/admin/create-user-form.component.ts
import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UsersService } from './users.service';
import type { UserRole } from '../../core/domain/user-session';

@Component({
  selector: 'app-create-user-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div data-testid="create-user-form" class="create-form">
      <h3>Create New User</h3>

      <div class="form-row">
        <label for="user-email">Email</label>
        <input
          id="user-email"
          data-testid="user-email-input"
          type="email"
          [(ngModel)]="email"
          placeholder="user@hclis.local"
        />
      </div>

      <div class="form-row">
        <label for="user-fullname">Full Name</label>
        <input
          id="user-fullname"
          data-testid="user-fullname-input"
          type="text"
          [(ngModel)]="fullName"
          placeholder="First Last"
        />
      </div>

      <div class="form-row">
        <label for="user-birthdate">Birthdate</label>
        <input
          id="user-birthdate"
          data-testid="user-birthdate-input"
          type="date"
          [(ngModel)]="birthdate"
        />
      </div>

      <div class="form-row">
        <label for="user-gender">Gender</label>
        <select id="user-gender" data-testid="user-gender-select" [(ngModel)]="gender">
          <option value="Male">Male</option>
          <option value="Female">Female</option>
          <option value="Other">Other</option>
        </select>
      </div>

      <div class="form-row">
        <label for="user-role">Role</label>
        <select id="user-role" data-testid="user-role-select" [(ngModel)]="role">
          <option value="Receptionist">Receptionist</option>
          <option value="LabTechnician">Lab Technician</option>
          <option value="Physician">Physician</option>
          <option value="ITAdmin">IT Admin</option>
        </select>
      </div>

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }

      <div class="form-actions">
        <button
          data-testid="submit-create-user-btn"
          (click)="onSubmit()"
          [disabled]="!email.trim() || !fullName.trim() || !birthdate"
        >
          Create User
        </button>
        <button class="cancel-btn" (click)="close.emit()">Cancel</button>
      </div>
    </div>
  `,
  styles: [`
    .create-form { padding: 1.5rem; border: 1px solid #ddd; border-radius: 6px; background: #fafafa; max-width: 420px; }
    .create-form h3 { margin: 0 0 1.2rem; }
    .form-row { display: flex; flex-direction: column; gap: 0.3rem; margin-bottom: 0.9rem; }
    .form-row label { font-size: 0.85rem; font-weight: 600; color: #444; }
    .form-row input, .form-row select { padding: 0.45rem 0.6rem; border: 1px solid #ccc; border-radius: 4px; font-size: 0.95rem; }
    .form-actions { display: flex; gap: 0.6rem; margin-top: 1rem; }
    .form-actions button { padding: 0.5rem 1.1rem; border: none; border-radius: 4px; cursor: pointer; }
    .form-actions button:first-child { background: #2c7be5; color: #fff; }
    .form-actions button:first-child:disabled { background: #9ab8e8; cursor: not-allowed; }
    .cancel-btn { background: #f0f0f0; color: #333; }
    .error { color: #c0392b; font-size: 0.9rem; }
  `],
})
export class CreateUserFormComponent {
  private readonly service = inject(UsersService);

  readonly close = output<void>();

  protected email = '';
  protected fullName = '';
  protected birthdate = '';
  protected gender = 'Male';
  protected role: UserRole = 'Receptionist';
  protected readonly error = signal<string | null>(null);

  protected async onSubmit(): Promise<void> {
    try {
      await this.service.createUser({
        email: this.email.trim(),
        fullName: this.fullName.trim(),
        birthdate: this.birthdate,
        gender: this.gender,
        role: this.role,
      });
      this.error.set(null);
      this.close.emit();
    } catch {
      this.error.set('Failed to create user. Please check the fields and try again.');
    }
  }
}
