import {Component, inject, input, signal, viewChild} from '@angular/core';
import { Router } from '@angular/router';
import { MoviePlannerClient } from '../../../planner-client';
import {TableModule} from 'primeng/table';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {ProgressSpinner} from 'primeng/progressspinner';
import {resourceObs} from '../../utils';
import {MessageService} from 'primeng/api';
import {DialogService} from 'primeng/dynamicdialog';
import {AddWatchlistItemComponent} from '../add-watchlist-item/add-watchlist-item.component';
import {Dialog} from 'primeng/dialog';

@Component({
  selector: 'app-watchlist',
  standalone: true,
  imports: [
    TableModule,
    ButtonDirective,
    ButtonIcon,
    ButtonLabel,
    ProgressSpinner,
    AddWatchlistItemComponent,
    Dialog

  ],
  templateUrl: './watchlist.component.html',
  styleUrl: './watchlist.component.css'
})
export class WatchlistComponent {
  private readonly client  = inject(MoviePlannerClient);
  private readonly messageService  = inject(MessageService);
  private readonly router  = inject(Router);
  private readonly addDialog = viewChild.required<Dialog>('addDialog');

  readonly userId = input.required<string>();
  readonly items =
    resourceObs(() => this.userId(), param => this.client.getWatchList(param))
  readonly dialogVisible = signal(false);

  generateWatchlist() {
    this.client.generateWatchList(this.userId(), 10).subscribe({
      next: _ => {
        this.items.reload();
        this.messageService.add({
          severity: 'success',
          summary: 'Watchlist generated',
          detail: `Generated watchlist!`
        });
      },
      error: err => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error generating watchlist',
          detail: err.error
        });
      }
    });
  }

  goTo(movieId: string) {
    this.router.navigate(['/planner/movies', movieId]);
  }

  openAdd() {
    this.dialogVisible.set(true)
  }
}
