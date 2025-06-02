{{ fullname | escape | underline}}

.. automodule:: {{ fullname }}
   {% block attributes %}
   {% if attributes %}
   .. rubric:: Module Attributes

   .. autosummary::
      :toctree:
   {% for item in attributes %}
      {{ item }}
   {%- endfor %}
   {% endif %}
   {% endblock %}

   {% block classes %}
   {% if classes %}
      {% if classes | length == 1 and functions | length == 0 and exceptions | length == 0 %}
         .. autoclass:: {{ classes[0] }}
            :members:
            :show-inheritance:
            :inherited-members:
            :member-order: bysource
      {% else %}
         .. rubric:: {{ _('Classes') }}

         .. autosummary::
            :template: class-template.rst
         {% for item in classes %}
            {{ item }}
         {%- endfor %}

      {% endif %}
   {% endif %}
   {% endblock %}

   {% block functions %}
   {% if functions %}
   .. rubric:: {{ _('Functions') }}

   .. autosummary::
      :toctree:
   {% for item in functions %}
      {{ item }}
   {%- endfor %}
   {% endif %}
   {% endblock %}

   {% block exceptions %}
   {% if exceptions %}
   .. rubric:: {{ _('Exceptions') }}

   .. autosummary::
      :toctree:
   {% for item in exceptions %}
      {{ item }}
   {%- endfor %}
   {% endif %}
   {% endblock %}

{% block modules %}
{% if modules %}
.. rubric:: Modules

.. autosummary::
   :toctree:
   :template: module-template.rst
   :recursive:
{% for item in modules %}
   {{ item }}
{%- endfor %}
{% endif %}
{% endblock %}
