import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { PhysiciansService } from './physicians.service';
import { PhysicianFormComponent } from './physician-form.component';
import type { PhysicianSearchResult } from '../../../core/domain/physician-search-result';
import { HcBadge } from '../../../ui/badge/badge';
import { HcButton } from '../../../ui/button/button';
import { HcDialog } from '../../../ui/dialog/dialog';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../../ui/dropdown-menu/dropdown-menu';
import { HcEmpty } from '../../../ui/empty/empty';
import { HcIcon } from '../../../ui/icon/icon';
import { HcPage } from '../../../ui/page/page';
import { HcPagination } from '../../../ui/pagination/pagination';
import { HcSkeleton, SKELETON_ROWS } from '../../../ui/skeleton/skeleton';
import { HcTable } from '../../../ui/table/table';
import { ToastService } from '../../../ui/toast/toast.service';

type StatusAction = 'deactivate' | 'reactivate';

interface PendingStatusChange {
  physicianId: string;
  fullName: string;
  action: StatusAction;
}

const PAGE_SIZE = 10;

const NoLicenceOnRecord = '—';

@Component({
  selector: 'app-physician-list',
  standalone: true,
  imports: [
    PhysicianFormComponent,
    HcBadge,
    HcButton,
    HcDialog,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcEmpty,
    HcIcon,
    HcPage,
    HcPagination,
    HcSkeleton,
    HcTable,
  ],
  templateUrl: './physician-list.component.html',
  styleUrl: './physician-list.component.css',
})
export class PhysicianListComponent implements OnInit {
  protected readonly service = inject(PhysiciansService);
  private readonly toast = inject(ToastService);

  protected readonly skeletonRows = SKELETON_ROWS;
  protected readonly noLicence = NoLicenceOnRecord;
  protected readonly page = signal(1);

  protected readonly formOpen = signal(false);
  /** The row the open form is editing; null while registering a new physician. */
  protected readonly editing = signal<PhysicianSearchResult | null>(null);

  protected readonly statusDialogOpen = signal(false);
  protected readonly pendingStatusChange = signal<PendingStatusChange | null>(null);
  protected readonly changingStatus = signal(false);

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.service.physicians().length / PAGE_SIZE)),
  );

  protected readonly displayPage = computed(() => Math.min(this.page(), this.pageCount()));

  protected readonly pagedPhysicians = computed<PhysicianSearchResult[]>(() => {
    const start = (this.displayPage() - 1) * PAGE_SIZE;
    return this.service.physicians().slice(start, start + PAGE_SIZE);
  });

  ngOnInit(): void {
    void this.service.listPhysicians();
  }

  protected openRegisterForm(): void {
    this.editing.set(null);
    this.formOpen.set(true);
  }

  protected openEditForm(physician: PhysicianSearchResult): void {
    this.editing.set(physician);
    this.formOpen.set(true);
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editing.set(null);
  }

  /** Esc and backdrop dismissals close the dialog on their own; the page still has to catch up. */
  protected onFormOpenChange(open: boolean): void {
    if (!open) {
      this.closeForm();
    }
  }

  protected onStatusDialogOpenChange(open: boolean): void {
    if (!open) {
      this.cancelStatusChange();
    }
  }

  protected requestStatusChange(physician: PhysicianSearchResult, action: StatusAction): void {
    this.pendingStatusChange.set({
      physicianId: physician.id,
      fullName: physician.fullName,
      action,
    });
    this.statusDialogOpen.set(true);
  }

  protected cancelStatusChange(): void {
    this.statusDialogOpen.set(false);
    this.pendingStatusChange.set(null);
  }

  protected async confirmStatusChange(): Promise<void> {
    const pending = this.pendingStatusChange();
    if (pending === null) {
      this.statusDialogOpen.set(false);
      return;
    }

    this.changingStatus.set(true);

    try {
      if (pending.action === 'deactivate') {
        await this.service.deactivate(pending.physicianId);
      } else {
        await this.service.reactivate(pending.physicianId);
      }
    } catch (error) {
      this.toast.show(`Could not ${pending.action} ${pending.fullName}. ${this.reasonFor(error)}`, {
        variant: 'error',
        testId: 'physician-status-error-toast',
      });
      return;
    } finally {
      this.changingStatus.set(false);
      this.statusDialogOpen.set(false);
      this.pendingStatusChange.set(null);
    }

    this.toast.show(
      pending.action === 'deactivate'
        ? `${pending.fullName} is no longer accepting new orders.`
        : `${pending.fullName} can be selected on new orders again.`,
      { variant: 'success', testId: 'physician-status-toast' },
    );
  }

  /** The server's explanation when it sent one, so the toast says how to move forward. */
  private reasonFor(error: unknown): string {
    const message = error instanceof Error ? error.message.trim() : '';
    return message.length > 0 ? message : 'Please try again.';
  }
}
