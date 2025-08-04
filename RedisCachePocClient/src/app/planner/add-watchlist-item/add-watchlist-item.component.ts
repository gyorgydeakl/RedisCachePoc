import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
  computed, output
} from '@angular/core';
import { MoviePlannerClient } from '../../../planner-client';
import { MovieReviewerClient, } from '../../../reviewer-client';
import { resourceObs, resourceObsNoParams } from '../../utils';
import { Select } from 'primeng/select';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Message } from 'primeng/message';
import {FormsModule} from '@angular/forms';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-add-watchlist',
  imports: [ Select, Button, ProgressSpinner, Message, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (user.isLoading() || allMovies.isLoading()) {
      <div class="flex justify-center items-center py-10">
        <p-progressSpinner class="w-8 h-8"/>
      </div>
    } @else if (!user.hasValue() || !allMovies.hasValue()) {
      <!-- Generic error view ---------------------------------------- -->
      <p-message severity="error">Something went wrong while loading data.</p-message>
    } @else {
      <!-- Normal view ----------------------------------------------- -->
      <div class="flex flex-col gap-6 flex-1">
        <h2 class="text-lg font-semibold">
          {{ user.value()?.username }} – choose a movie
        </h2>

        <!-- Movie selector -->
        <p-select
          class="w-full"
          placeholder="Select movie"
          [options]="allMovies.value()"
          optionLabel="title"
          optionValue="id"
          [ngModel]="selectedMovieId()"
          (onChange)="selectedMovieId.set($event.value)"
        />

        <!-- Submit button -->
        <p-button
          label="Add"
          class="w-full"
          [loading]="isSubmitting()"
          [disabled]="!canSubmit()"
          (onClick)="addToWatchlist()"
        />
      </div>
    }
  `
})
export class AddWatchlistItemComponent {
  private readonly planner = inject(MoviePlannerClient);
  private readonly reviewer = inject(MovieReviewerClient);
  private readonly msg = inject(MessageService);

  readonly userId = input.required<string>();

  readonly user = resourceObs(
    () => this.userId(),
    id => this.planner.getUser(id)
  );

  readonly allMovies = resourceObsNoParams(() => this.reviewer.moviesGet());
  readonly userAdded = output()
  readonly selectedMovieId = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly canSubmit = computed(
    () => !!this.selectedMovieId() && !this.isSubmitting()
  );

  async addToWatchlist(){
    if (!this.canSubmit()) return;

    this.isSubmitting.set(true);
    try {
      this.planner.addWatchListItem({
        userId:this.userId(),
        movieId: this.selectedMovieId()!
      }).subscribe({
        next: _ => {
          this.user.reload();
          this.userAdded.emit();
          this.msg.add({
            severity: 'success',
            summary: 'Movie added to watchlist',
            detail: `Movie '${this.selectedMovieId()}' added to watchlist!`
          })
        },
        error: err => {
          this.msg.add({
            severity: 'error',
            summary: 'Error adding movie to watchlist',
            detail: err.error
          })
        }
      });
    } finally {
      this.isSubmitting.set(false);
    }
  };
}
