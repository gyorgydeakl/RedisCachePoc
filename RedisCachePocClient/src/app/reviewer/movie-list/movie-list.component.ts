import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MovieReviewerClient, MovieSummaryDto } from '../../../reviewer-client';

/* PrimeNG */
import { Dialog } from 'primeng/dialog';
import { DataView } from 'primeng/dataview';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import { PrimeTemplate } from 'primeng/api';

/* Your form component */
import { AddMovieComponent } from '../add-movie/add-movie.component';
import {resourceObsNoParams} from '../../utils';
import {ProgressSpinner} from 'primeng/progressspinner';

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [
    Dialog,
    DataView,
    Card,
    Tag,
    PrimeTemplate,
    AddMovieComponent,
    ButtonDirective,
    ButtonLabel,
    ButtonIcon,
    ProgressSpinner,
  ],
  templateUrl: './movie-list.component.html',
  styleUrl: './movie-list.component.css',
})
export class MovieListComponent {
  private readonly client = inject(MovieReviewerClient);
  private readonly router = inject(Router);

  protected readonly movies = resourceObsNoParams(() => this.client.moviesGet());
  protected readonly showAdd = signal(false);

  goTo(id: string) {
    this.router.navigate(['reviewer/movies', id]);
  }

  openAdd() {
    this.showAdd.set(true);
  }
  closedAdd() {
    this.movies.reload();
  }

  onMovieCreated() {
    this.showAdd.set(false);
    this.movies.reload();
  }

  generateMovies() {
    this.client.generateMovies(10).subscribe(r => {
      this.movies.reload();
      this.showAdd.set(false);
    });
  }
}
