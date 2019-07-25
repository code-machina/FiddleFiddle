# Trouble Shootings

## 3/18/2018

ISSUES : django.db.utils.ProgrammingError: Can't adapt type 'dict'

See, https://stackoverflow.com/questions/29557782/django-1-8-hstore-field-throwing-progamming-error

<pre><code>
Ensure you add 'django.contrib.postgres' to settings.INSTALLED_APPS.
</code></pre>