import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { WorklistService } from './worklist.service';
import { WorklistItemDetailComponent } from './worklist-item-detail.component';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcDateTimePipe } from '../../ui/date/hc-datetime.pipe';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcPage } from '../../ui/page/page';
import { HcPagination } from '../../ui/pagination/pagination';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcSkeleton } from '../../ui/skeleton/skeleton';
import { HcTable } from '../../ui/table/table';
import type { WorklistItemSummary } from '../../core/domain/worklist-item-summary';

type SortKey = 'sampleBarcode' | 'examCode' | 'patientName' | 'status' | 'createdAt';
type SortDir = 'asc' | 'desc';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-worklist',
  standalone: true,
  imports: [
    WorklistItemDetailComponent,
    HcBadge,
    HcButton,
    HcDateTimePipe,
    HcEmpty,
    HcIcon,
    HcPage,
    HcPagination,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcSkeleton,
    HcTable,
  ],
  templateUrl: './worklist.component.html',
  styleUrl: './worklist.component.css',
})
export class WorklistComponent implements OnInit {
  protected readonly service = inject(WorklistService);
  protected readonly skeletonRows = Array.from({ length: 5 });

  protected readonly sortKey = signal<SortKey | null>(null);
  protected readonly sortDir = signal<SortDir>('asc');
  protected readonly page = signal(1);

  protected readonly sorted = computed<WorklistItemSummary[]>(() => {
    const list = this.service.items();
    const key = this.sortKey();
    if (!key) {
      return list;
    }
    const dir = this.sortDir() === 'asc' ? 1 : -1;
    return [...list].sort((a, b) => compareBy(a, b, key) * dir);
  });

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.sorted().length / PAGE_SIZE)),
  );

  protected readonly displayPage = computed(() => Math.min(this.page(), this.pageCount()));

  protected readonly pagedItems = computed<WorklistItemSummary[]>(() => {
    const start = (this.displayPage() - 1) * PAGE_SIZE;
    return this.sorted().slice(start, start + PAGE_SIZE);
  });

  ngOnInit(): void {
    void this.service.loadItems();
  }

  protected refresh(): void {
    void this.service.loadItems();
  }

  protected onRowClick(id: string): void {
    void this.service.getItemDetails(id);
  }

  protected toggleSort(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortDir.update(d => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortKey.set(key);
      this.sortDir.set('asc');
    }
    this.page.set(1);
  }

  protected ariaSort(key: SortKey): 'ascending' | 'descending' | 'none' {
    if (this.sortKey() !== key) {
      return 'none';
    }
    return this.sortDir() === 'asc' ? 'ascending' : 'descending';
  }
}

function compareBy(a: WorklistItemSummary, b: WorklistItemSummary, key: SortKey): number {
  const av = a[key];
  const bv = b[key];
  return String(av ?? '').localeCompare(String(bv ?? ''));
}
