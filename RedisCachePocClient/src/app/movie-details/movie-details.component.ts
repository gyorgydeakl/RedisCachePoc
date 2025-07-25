import {Component, inject, input, OnInit, signal} from '@angular/core';
import {MovieDetailsDto, MovieReviewerClient, ReviewDto} from '../../client';
import {Card} from 'primeng/card';
import {PrimeTemplate} from 'primeng/api';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink} from '@angular/router';
import {Tag} from 'primeng/tag';
import {AddReviewComponent} from '../add-review/add-review.component';

@Component({
  selector: 'app-movie-details',
  imports: [
    Card,
    PrimeTemplate,
    ButtonDirective,
    RouterLink,
    ButtonLabel,
    ButtonIcon,
    Tag,
    AddReviewComponent
  ],
  templateUrl: './movie-details.component.html',
  styleUrl: './movie-details.component.css'
})
export class MovieDetailsComponent implements OnInit {
  private readonly client = inject(MovieReviewerClient);

  readonly movieId = input.required<string>()
  protected readonly movie = signal<MovieDetailsDto | null>(null);

  ngOnInit(): void {
    this.client.moviesIdGet(this.movieId()).subscribe(movie => this.movie.set(movie));
  }

  addReview(r: ReviewDto) {
    const current = this.movie();
    if (!current) return;

    this.movie.set({
      ...current,
      reviews: [...(current.reviews ?? []), r]
    });
  }
}
