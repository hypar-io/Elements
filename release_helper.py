import json
import os
import requests
import sys

# Notes:
# Makes some assumptions in getting the commits: uses the published at date of the last release
# as the "since" argument to get commits since then. In theory this may exclude merge commits that happened
# in between the creation and the publish, although this shouldn't be happening. Not using the created at date
# because was including commits from last release.
# This is also using the "list commits" rather than "compare" functionality, as it is lighter weight and also
# simpler to paginate. This may mean that our "number of commits since last release" may not be identical in count,
# but the logic by checking on dates when we only care about merge commits should be sound.
# See discussion here for jumping off point:
# https://stackoverflow.com/questions/14420076/github-v3-api-get-full-commit-list-for-large-comparison

github_token = os.environ.get('GITHUB_TOKEN')
repository = os.environ.get('REPOSITORY') or 'Elements'
max_pages_to_read = int(os.environ.get('MAX_COMMIT_PAGES') or '5')

page_size = 100
merge_commits = []

if github_token is None:
    sys.exit('Github token is required to run this helper')


def get_url(url):
    print('requesting:', url)
    return requests.get(url, auth=('token', github_token))


def get_latest_commit():
    response = get_url(f'https://api.github.com/repos/hypar-io/{repository}/releases/latest')
    latest_release_json = json.loads(response.text)
    return {
        'name': latest_release_json['tag_name'],
        'published_at': latest_release_json['published_at']
    }


def add_relevant_commits(response):
    commits = json.loads(response.text)
    for commit in commits:
        message = commit.get('commit').get('message')
        if(message.startswith('Merge pull request')):
            split_message = message.split('\n\n')
            if len(split_message) > 1:
                merge_commits.append(split_message[0].replace('Merge pull request ', '') + ': ' + split_message[1])


latest_release_tag = get_latest_commit()

if latest_release_tag is None:
    print('No latest release found')
else:
    print('---')
    print('Latest release:', latest_release_tag['name'])
    print(latest_release_tag['published_at'])
    print('---')
    if latest_release_tag['published_at'] is None:
        sys.exit('Unable to parse creation date of last tag')
    requests_sent = 0
    # In the future if we want to specify branch, we can by passing in 'sha' query. See:
    # https://docs.github.com/en/free-pro-team@latest/rest/reference/repos#commits
    url = f'https://api.github.com/repos/hypar-io/{repository}/commits?since=' + latest_release_tag['published_at'] + '&per_page=' + str(page_size)
    response = get_url(url)
    requests_sent += 1
    add_relevant_commits(response)
    while next := (response and response.links and response.links.get('next') and response.links.get('next').get('url')):
        if requests_sent >= max_pages_to_read:
            break
        response = get_url(next)
        add_relevant_commits(response)
        requests_sent += 1
    if next:
        print('---')
        print('We ran our maximum number of queries, but did not reach the end of the commits list. Please raise the limits to include our expected last page:')
        print(response and response.links and response.links['last'] and response.links['last']['url'])
        sys.exit('Did not fetch all of our applicable commits successfully.')
    else:
        print('---')
        print('All Merge Commits Since', latest_release_tag['published_at'])
        print('---')
        for commit in merge_commits:
            print(commit)
