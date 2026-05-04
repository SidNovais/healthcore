import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiError } from '@hc-lis/api-client';
import { AuthService } from '../../core/application/auth.service';
import type { LoginCredentials } from '../../core/application/i-auth-port';
import { ROLE_HOME_ROUTE } from '../../core/domain/user-session';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(8)]),
  });

  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(false);

  async submit(): Promise<void> {
    if (this.form.invalid) return;
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const creds = this.form.getRawValue() as LoginCredentials;
      await this.authService.login(creds);
      const role = this.authService.currentUser()!.role;
      await this.router.navigate([ROLE_HOME_ROUTE[role]]);
    } catch (err) {
      const message = err instanceof ApiError ? err.detail : 'An unexpected error occurred. Please try again.';
      this.errorMessage.set(message);
    } finally {
      this.isLoading.set(false);
    }
  }
}
