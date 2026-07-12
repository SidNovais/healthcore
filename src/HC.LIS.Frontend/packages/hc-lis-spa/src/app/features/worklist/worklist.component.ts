import { Component, OnInit, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { WorklistService } from './worklist.service';
import { WorklistItemDetailComponent } from './worklist-item-detail.component';
import { HcBadge } from '../../ui/badge/badge';
import { HcButton } from '../../ui/button/button';
import { HcEmpty } from '../../ui/empty/empty';
import { HcIcon } from '../../ui/icon/icon';
import { HcTable } from '../../ui/table/table';

@Component({
  selector: 'app-worklist',
  standalone: true,
  imports: [WorklistItemDetailComponent, SlicePipe, HcBadge, HcButton, HcEmpty, HcIcon, HcTable],
  templateUrl: './worklist.component.html',
  styleUrl: './worklist.component.css',
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
