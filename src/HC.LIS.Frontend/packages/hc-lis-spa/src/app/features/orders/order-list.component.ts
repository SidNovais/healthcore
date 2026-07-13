import { Component, ElementRef, OnInit, effect, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { gsap } from 'gsap';
import { OrdersService } from './orders.service';
import { HcBadge } from '../../ui/badge/badge';
import { HcEmpty } from '../../ui/empty/empty';
import { HcSkeleton } from '../../ui/skeleton/skeleton';
import { HcTable } from '../../ui/table/table';
import { MOTION, prefersReducedMotion } from '../../ui/motion/motion';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [RouterLink, HcBadge, HcEmpty, HcSkeleton, HcTable],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css',
})
export class OrderListComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);
  protected readonly skeletonRows = Array.from({ length: 5 });
  private readonly host = inject(ElementRef).nativeElement as HTMLElement;

  constructor() {
    // Stagger rows in whenever a fresh (non-empty) result set renders.
    effect(() => {
      const count = this.ordersService.orderList().length;
      if (count === 0 || prefersReducedMotion()) return;
      requestAnimationFrame(() => {
        const rows = this.host.querySelectorAll('[data-testid="order-list-row"]');
        if (rows.length === 0) return;
        gsap.from(rows, { autoAlpha: 0, y: 8, duration: MOTION.fast, stagger: 0.03, overwrite: true });
      });
    });
  }

  ngOnInit(): void {
    void this.ordersService.loadOrderList();
  }
}
