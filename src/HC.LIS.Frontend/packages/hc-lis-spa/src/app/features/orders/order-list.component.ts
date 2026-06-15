import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrdersService } from './orders.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <h1 class="page-title">Orders</h1>
      </div>

      <table data-testid="order-list-table" class="orders-table">
        <thead>
          <tr>
            <th>Patient</th>
            <th>Requested By</th>
            <th>Priority</th>
            <th>Requested At</th>
            <th>Items</th>
          </tr>
        </thead>
        <tbody>
          @if (ordersService.orderList().length === 0) {
            <tr>
              <td colspan="5" class="empty-cell">No orders found.</td>
            </tr>
          } @else {
            @for (item of ordersService.orderList(); track item.orderId) {
              <tr
                data-testid="order-list-row"
                [routerLink]="['/orders', item.orderId]"
                class="order-row"
              >
                <td data-testid="patient-name-cell">{{ item.patientName ?? item.patientId }}</td>
                <td>{{ item.requestedBy }}</td>
                <td>{{ item.orderPriority }}</td>
                <td>{{ item.requestedAt }}</td>
                <td>{{ item.itemCount }}</td>
              </tr>
            }
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-header { margin-bottom: 1.5rem; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
    .orders-table { width: 100%; border-collapse: collapse; }
    .orders-table th { text-align: left; padding: 0.5rem 0.75rem; border-bottom: 2px solid #e5e7eb; font-size: 0.75rem; text-transform: uppercase; color: #6b7280; }
    .orders-table td { padding: 0.75rem; border-bottom: 1px solid #f3f4f6; font-size: 0.875rem; }
    .order-row { cursor: pointer; }
    .order-row:hover td { background: #f9fafb; }
    .empty-cell { text-align: center; color: #9ca3af; padding: 2rem; }
  `],
})
export class OrderListComponent implements OnInit {
  protected readonly ordersService = inject(OrdersService);

  ngOnInit(): void {
    void this.ordersService.loadOrderList();
  }
}
