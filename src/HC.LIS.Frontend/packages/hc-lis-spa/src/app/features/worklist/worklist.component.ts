import { Component, OnInit, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { WorklistService } from './worklist.service';
import { WorklistItemDetailComponent } from './worklist-item-detail.component';

@Component({
  selector: 'app-worklist',
  standalone: true,
  imports: [WorklistItemDetailComponent, SlicePipe],
  template: `
    <div class="worklist-page">
      <div class="worklist-header">
        <h2 data-testid="worklist-title">Doctor Worklist</h2>
        <button data-testid="refresh-btn" (click)="refresh()">Refresh</button>
      </div>

      <div class="worklist-layout">
        <div class="worklist-table-container">
          @if (service.items().length === 0) {
            <p data-testid="empty-state" class="empty-state">No worklist items found.</p>
          } @else {
            <table class="worklist-table">
              <thead>
                <tr>
                  <th>Barcode</th>
                  <th>Exam</th>
                  <th>Patient</th>
                  <th>Status</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                @for (item of service.items(); track item.id) {
                  <tr
                    data-testid="worklist-row"
                    class="worklist-row"
                    [class.selected]="service.selectedItem()?.id === item.id"
                    (click)="onRowClick(item.id)"
                  >
                    <td>{{ item.sampleBarcode }}</td>
                    <td>{{ item.examCode }}</td>
                    <td data-testid="patient-name-cell">{{ item.patientName ?? item.patientId }}</td>
                    <td><span class="status-badge">{{ item.status }}</span></td>
                    <td>{{ item.createdAt | slice: 0 : 10 }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        @if (service.selectedItem(); as detail) {
          <app-worklist-item-detail [item]="detail" />
        }
      </div>
    </div>
  `,
  styles: [`
    .worklist-page { padding: 2rem; }
    .worklist-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
    .worklist-header h2 { margin: 0; }
    .worklist-header button { padding: 0.4rem 0.9rem; border: 1px solid #ccc; border-radius: 4px; cursor: pointer; }
    .worklist-layout { display: flex; gap: 2rem; }
    .worklist-table-container { flex: 1; }
    .empty-state { color: var(--color-text-muted, #888); }
    .worklist-table { width: 100%; border-collapse: collapse; }
    .worklist-table th, .worklist-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid #e0e0e0; text-align: left; }
    .worklist-row { cursor: pointer; }
    .worklist-row:hover { background: #f5f9ff; }
    .worklist-row.selected { background: #e8f0fe; }
    .status-badge { background: #f0f0f0; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.82rem; }
  `],
})
export class WorklistComponent implements OnInit {
  protected readonly service = inject(WorklistService);

  ngOnInit(): void {
    void this.service.loadItems();
  }

  protected refresh(): void {
    void this.service.loadItems();
  }

  protected onRowClick(id: string): void {
    void this.service.getItemDetails(id);
  }
}
