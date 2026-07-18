import { Component, Input, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorklistService } from './worklist.service';
import { AuthService } from '../../core/application/auth.service';
import type { WorklistItemDetails } from '../../core/domain/worklist-item-details';
import { HcAlert } from '../../ui/alert/alert';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcDatePipe } from '../../ui/date/hc-date.pipe';
import { HcField } from '../../ui/field/field';
import { HcInput } from '../../ui/input/input';
import { HcLabel } from '../../ui/input/label';
import { HcTable } from '../../ui/table/table';

@Component({
  selector: 'app-worklist-item-detail',
  standalone: true,
  imports: [FormsModule, HcAlert, HcBadge, HcButton, HcDatePipe, HcField, HcInput, HcLabel, HcTable],
  templateUrl: './worklist-item-detail.component.html',
  styleUrl: './worklist-item-detail.component.css',
})
export class WorklistItemDetailComponent {
  @Input({ required: true }) item!: WorklistItemDetails;

  private readonly service = inject(WorklistService);
  private readonly auth = inject(AuthService);

  protected signature = '';
  protected readonly reportSigned = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async onSignReport(): Promise<void> {
    const userId = this.auth.currentUser()?.userId ?? '';
    try {
      await this.service.signReport(this.item.id, {
        signature: this.signature.trim(),
        signedBy: userId,
      });
      this.reportSigned.set(true);
      this.error.set(null);
    } catch {
      this.error.set('Failed to sign report. Please try again.');
    }
  }
}
