import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { gsap } from 'gsap';
import { ApiError } from '@hc-lis/api-client';
import { AuthService } from '../../core/application/auth.service';
import type { LoginCredentials } from '../../core/application/i-auth-port';
import { ROLE_HOME_ROUTE } from '../../core/domain/user-session';
import { HcAlert } from '../../ui/alert/alert';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcField } from '../../ui/field/field';
import { HcIcon } from '../../ui/icon/icon';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';
import { MOTION, useMotion } from '../../ui/motion/motion';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, HcAlert, HcButton, HcCard, HcField, HcIcon, HcInput, HcLabel],
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

  // Zoneless: control validity/touched aren't signals, so bridge each control's
  // event stream into a signal to make the error computeds reactive.
  private readonly emailEvents = toSignal(this.form.controls.email.events);
  private readonly passwordEvents = toSignal(this.form.controls.password.events);

  readonly emailError = computed(() => {
    this.emailEvents();
    const control = this.form.controls.email;
    if (!(control.touched && control.invalid)) return null;
    return control.hasError('required')
      ? 'Email address is required'
      : 'Enter a valid email address';
  });

  readonly passwordError = computed(() => {
    this.passwordEvents();
    const control = this.form.controls.password;
    if (!(control.touched && control.invalid)) return null;
    return control.hasError('required')
      ? 'Password is required'
      : 'Password must be at least 8 characters';
  });

  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(false);

  constructor() {
    useMotion(({ reduceMotion }) => {
      if (reduceMotion) return;
      gsap.from('.login-card', { autoAlpha: 0, y: 12, duration: MOTION.slow });
    });
  }

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
