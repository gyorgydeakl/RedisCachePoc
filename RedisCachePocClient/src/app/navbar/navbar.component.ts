import {Component, inject} from '@angular/core';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink} from '@angular/router';
import {MovieReviewerClient} from '../../reviewer-client';
import {MessageService} from 'primeng/api';
import {MoviePlannerClient} from '../../planner-client';
import {DialogService, DynamicDialogRef} from 'primeng/dynamicdialog';
import {ViewCacheComponent} from '../view-cache/view-cache.component';

@Component({
  selector: 'app-navbar',
  imports: [
    ButtonIcon,
    ButtonLabel,
    ButtonDirective,
    RouterLink
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  private readonly reviewerClient = inject(MovieReviewerClient)
  private readonly dialogService = inject(DialogService)
  private readonly messageService = inject(MessageService)

  clearDb() {
    this.reviewerClient.clearDatabase().subscribe(
      _ => this.messageService.add({
        severity: 'success',
        summary: 'Database cleared',
        detail: 'Database cleared!'
      })
    )
  }

  viewCache() {
    this.dialogService.open(ViewCacheComponent, { header: 'Cache', closable: true, width: '80vw', height: '80vh' });
  }
}
