// src/app/features/admin/create-user-form.component.ts
import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UsersService } from './users.service';
import type { UserRole } from '../../core/domain/user-session';
import { HcAlert } from '../../ui/alert/alert';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcField } from '../../ui/field/field';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';
import { HcSelect } from '../../ui/select/select';

@Component({
  selector: 'app-create-user-form',
  standalone: true,
  imports: [FormsModule, HcAlert, HcButton, HcCard, HcField, HcInput, HcLabel, HcSelect],
  templateUrl: './create-user-form.component.html',
  styleUrl: './create-user-form.component.css',
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
