import { Component, inject } from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import { MoviePlannerClient } from '../../../planner-client';
import {MovieReviewerClient} from '../../../reviewer-client';

import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {TableModule} from 'primeng/table';
import {resourceObsNoParams} from '../../utils';
import {ProgressSpinner} from 'primeng/progressspinner';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [ButtonDirective, ButtonLabel, TableModule, ButtonIcon, ProgressSpinner, RouterLink],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent {
  private readonly plannerClient  = inject(MoviePlannerClient);
  private readonly reviewerClient  = inject(MovieReviewerClient);
  private readonly messageService  = inject(MessageService);
  private readonly router  = inject(Router);

  readonly users = resourceObsNoParams(() => this.plannerClient.getUsers());

  view(id: string) {
    this.router.navigate(['planner/users', id]);
  }

  generateUsers() {
    this.reviewerClient.generateUsers(10).subscribe(r => {
      this.users.reload();
      this.messageService.add({
        severity: 'success',
        summary: 'Users generated',
        detail: `Generated users!`
      });
    })
  }
}
