import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { PrintLabelsCardComponent } from './print-labels-card.component';
import { TriageService } from './triage.service';
import type { SampleSummary } from '../../core/domain/sample-summary';
import { HcButton } from '../../ui/button/button';
import { HcIcon } from '../../ui/icon/icon';
import { MOTION, useMotion } from '../../ui/motion/motion';
import { gsap } from 'gsap';

@Component({
  selector: 'app-print-labels-modal',
  standalone: true,
  imports: [PrintLabelsCardComponent, HcButton, HcIcon],
  templateUrl: './print-labels-modal.component.html',
  styleUrl: './print-labels-modal.component.css',
})
export class PrintLabelsModalComponent implements OnInit {
  private readonly triageService = inject(TriageService);

  constructor() {
    useMotion(ctx => {
      if (ctx.reduceMotion) return;
      gsap.from('.modal-box', { autoAlpha: 0, scale: 0.98, duration: MOTION.normal, ease: 'power2.out' });
    });
  }

  readonly collectionRequestId = input.required<string>();
  readonly patientId = input.required<string>();
  readonly closed = output<void>();

  protected readonly samples = signal<SampleSummary[]>([]);
  protected readonly loading = signal(true);

  async ngOnInit(): Promise<void> {
    try {
      const result = await this.triageService.getSamples(this.collectionRequestId());
      this.samples.set(result);
    } finally {
      this.loading.set(false);
    }
  }

  protected onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('overlay')) {
      this.closed.emit();
    }
  }

  protected printLabels(): void {
    document.body.classList.add('printing-labels');
    window.addEventListener('afterprint', () => {
      document.body.classList.remove('printing-labels');
    }, { once: true });
    window.print();
  }
}
