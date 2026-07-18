import { Component, ElementRef, OnInit, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { gsap } from 'gsap';
import { OrdersService } from './orders.service';
import { HcBadge } from '../../ui/badge/badge';
import { HcDateTimePipe } from '../../ui/date/hc-datetime.pipe';
import { HcEmpty } from '../../ui/empty/empty';
import { HcSkeleton } from '../../ui/skeleton/skeleton';
import { HcTable } from '../../ui/table/table';
import { HcPage } from '../../ui/page/page';
import { HcPagination } from '../../ui/pagination/pagination';
import {
  HcDropdownMenu,
  HcDropdownMenuItem,
  HcDropdownMenuTrigger,
} from '../../ui/dropdown-menu/dropdown-menu';
import { HcIcon } from '../../ui/icon/icon';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';
import type { OrderListItem } from '../../core/domain/order-list-item';

type SortKey = 'patientName' | 'requestedBy' | 'orderPriority' | 'requestedAt' | 'itemCount';
type SortDir = 'asc' | 'desc';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    RouterLink,
    HcBadge,
    HcDateTimePipe,
    HcEmpty,
    HcPage,
    HcSkeleton,
    HcTable,
    HcPagination,
    HcDropdownMenu,
    HcDropdownMenuTrigger,
    HcDropdownMenuItem,
    HcIcon,
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css',
})
export class OrderListComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  protected readonly skeletonRows = Array.from({ length: 5 });

  protected readonly sortKey = signal<SortKey | null>(null);
  protected readonly sortDir = signal<SortDir>('asc');
  protected readonly page = signal(1);

  protected readonly sorted = computed<OrderListItem[]>(() => {
    const list = this.ordersService.orderList();
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

  /** Current page clamped into range so a shrinking result set can't strand us. */
  protected readonly displayPage = computed(() => Math.min(this.page(), this.pageCount()));

  protected readonly pagedOrders = computed<OrderListItem[]>(() => {
    const start = (this.displayPage() - 1) * PAGE_SIZE;
    return this.sorted().slice(start, start + PAGE_SIZE);
  });

  constructor() {
    // Stagger rows in whenever a fresh (non-empty) page renders.
    effect(() => {
      const count = this.pagedOrders().length;
      if (count === 0 || prefersReducedMotion()) {
        return;
      }
      requestAnimationFrame(() => {
        const rows = this.host.querySelectorAll('[data-testid="order-list-row"]');
        if (rows.length === 0) {
          return;
        }
        gsap.from(rows, { autoAlpha: 0, y: 8, duration: MOTION.fast, stagger: 0.03, overwrite: true });
      });
    });
  }

  ngOnInit(): void {
    void this.ordersService.loadOrderList();
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

  protected viewOrder(orderId: string): void {
    void this.router.navigate(['/orders', orderId]);
  }
}

function compareBy(a: OrderListItem, b: OrderListItem, key: SortKey): number {
  const av = a[key];
  const bv = b[key];
  if (typeof av === 'number' && typeof bv === 'number') {
    return av - bv;
  }
  return String(av ?? '').localeCompare(String(bv ?? ''));
}
