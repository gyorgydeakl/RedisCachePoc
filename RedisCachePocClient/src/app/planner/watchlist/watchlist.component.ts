import { Component, inject, input, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MoviePlannerClient, WatchListItemDto } from '../../../planner-client';
import {TableModule} from 'primeng/table';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';

@Component({
  selector: 'app-watchlist',
  standalone: true,
  imports: [
    TableModule,
    ButtonDirective,
    ButtonIcon,
    ButtonLabel

  ],
  templateUrl: './watchlist.component.html',
  styleUrl: './watchlist.component.css'
})
export class WatchlistComponent implements OnInit {
  private readonly client  = inject(MoviePlannerClient);
  private readonly router  = inject(Router);

  /** Parent supplies user id */
  userId = input.required<string>();

  /** Reactive data store */
  readonly items = signal<WatchListItemDto[]>([]);

  ngOnInit() {
    this.loadWatchlist();
  }

  loadWatchlist() {
    this.client.getWatchList(this.userId()).subscribe(i => this.items.set(i));
  }

  goTo(movieId: string) {
    this.router.navigate(['/reviewer/movies', movieId]);
  }
}
