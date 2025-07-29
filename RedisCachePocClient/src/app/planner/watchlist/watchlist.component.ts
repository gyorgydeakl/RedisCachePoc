import {Component, inject, input, OnInit, resource, signal} from '@angular/core';
import { Router } from '@angular/router';
import { MoviePlannerClient, WatchlistItemDto } from '../../../planner-client';
import {TableModule} from 'primeng/table';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {firstValueFrom} from 'rxjs';
import {ProgressSpinner} from 'primeng/progressspinner';
import {resourceObs} from '../../utils';

@Component({
  selector: 'app-watchlist',
  standalone: true,
  imports: [
    TableModule,
    ButtonDirective,
    ButtonIcon,
    ButtonLabel,
    ProgressSpinner

  ],
  templateUrl: './watchlist.component.html',
  styleUrl: './watchlist.component.css'
})
export class WatchlistComponent {
  private readonly client  = inject(MoviePlannerClient);
  private readonly router  = inject(Router);

  readonly userId = input.required<string>();
  readonly items =
    resourceObs(() => this.userId(), param => this.client.getWatchList(param))

  generateWatchlist() {
    this.client.generateWatchList(this.userId(), 10).subscribe(_ => this.items.reload());
  }

  goTo(movieId: string) {
    this.router.navigate(['/planner/movies', movieId]);
  }
}
