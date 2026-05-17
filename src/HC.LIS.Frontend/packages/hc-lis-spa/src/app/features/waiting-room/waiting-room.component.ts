import { Component, inject, OnInit } from '@angular/core';
import { CollectionRequestsService } from './collection-requests.service';
import { PatientCardComponent } from './patient-card.component';
import { AuthService } from '../../core/application/auth.service';
import type { CollectSampleFormData } from './collect-sample-form.component';

@Component({
  selector: 'app-waiting-room',
  standalone: true,
  imports: [PatientCardComponent],
  template: `
    <div class="page">
      <div class="page-header">
        <h1 data-testid="waiting-room-title" class="page-title">Waiting Room</h1>
        <button class="btn-refresh" (click)="refresh()">Refresh</button>
      </div>

      @if (service.queue().length === 0) {
        <div data-testid="empty-state" class="empty-state">
          <p>No patients waiting.</p>
        </div>
      } @else {
        <div class="queue-list">
          @for (item of service.queue(); track item.collectionRequestId) {
            <app-patient-card
              [item]="item"
              (callClicked)="onCall($event)"
              (collectSubmitted)="onCollect($event.id, $event.data)"
            />
          }
        </div>
      }

      @if (error()) {
        <p role="alert" class="error-text">{{ error() }}</p>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 2rem auto; padding: 0 1.5rem; font-family: system-ui, sans-serif; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.5rem; }
    .page-title { font-size: 1.5rem; font-weight: 600; color: #111; margin: 0; }
    .btn-refresh { padding: 0.4rem 0.9rem; background: #f3f4f6; border: 1px solid #d1d5db; border-radius: 2px; font-size: 0.8rem; cursor: pointer; color: #374151; }
    .btn-refresh:hover { background: #e5e7eb; }
    .queue-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .empty-state { padding: 3rem; text-align: center; color: #9ca3af; border: 1px dashed #d1d5db; }
    .error-text { color: #b91c1c; font-size: 0.875rem; margin-top: 1rem; }
  `],
})
export class WaitingRoomComponent implements OnInit {
  protected readonly service = inject(CollectionRequestsService);
  private readonly auth = inject(AuthService);

  protected error = () => null as string | null;

  ngOnInit(): void {
    void this.service.loadQueue();
  }

  protected async refresh(): Promise<void> {
    await this.service.loadQueue();
  }

  protected async onCall(id: string): Promise<void> {
    try {
      await this.service.callPatient(id);
      await this.service.loadQueue();
    } catch (err) {
      console.error('Failed to call patient', err);
    }
  }

  protected async onCollect(id: string, data: CollectSampleFormData): Promise<void> {
    const technicianId = this.auth.currentUser()?.userId ?? '';
    try {
      await this.service.recordCollection(id, {
        sampleId: crypto.randomUUID(),
        patientName: data.patientName,
        patientBirthdate: data.patientBirthdate,
        patientGender: data.patientGender,
        technicianId,
      });
      await this.service.loadQueue();
    } catch (err) {
      console.error('Failed to record collection', err);
    }
  }
}
